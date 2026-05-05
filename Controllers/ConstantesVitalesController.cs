using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ambulancia_MIS.Data;
using Ambulancia_MIS.Dominio;
using Ambulancia_MIS.DTOs;

namespace Ambulancia_MIS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConstantesVitalesController : ControllerBase
    {
        private readonly AmbulanciaContext _context;

        public ConstantesVitalesController(AmbulanciaContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/ConstantesVitales/mision/1
        /// Obtiene todas las constantes vitales de una misión
        /// </summary>
        [HttpGet("mision/{idMision}")]
        public async Task<IActionResult> GetConstantesPorMision(long idMision)
        {
            // Verificar si la misión existe
            var misionExiste = await _context.Misiones.AnyAsync(m => m.IdMision == idMision);
            if (!misionExiste)
                return NotFound($"La misión con ID {idMision} no existe");

            var constantes = await _context.ConstantesVitales
                .Where(c => c.IdMision == idMision)
                .OrderByDescending(c => c.FechaRegistro)
                .Select(c => new ConstanteVitalDTO
                {
                    FechaRegistro = c.FechaRegistro,
                    FrecuenciaCardiaca = c.FrecuenciaCardiaca,
                    PresionArterialSistolica = c.PresionSistolica,
                    PresionArterialDiastolica = c.PresionDiastolica,
                    SaturacionOxigeno = c.SaturacionOxigeno,
                    Temperatura = c.Temperatura
                })
                .ToListAsync();

            // Calcular tendencia si hay al menos 2 registros
            var tendencia = ObtenerTendencia(constantes);

            return Ok(new
            {
                IdMision = idMision,
                TotalRegistros = constantes.Count,
                UltimoRegistro = constantes.FirstOrDefault(),
                Tendencia = tendencia,
                Constantes = constantes
            });
        }

        /// <summary>
        /// GET: api/ConstantesVitales/ultimo/{idMision}
        /// Obtiene solo la última constante vital registrada
        /// </summary>
        [HttpGet("ultimo/{idMision}")]
        public async Task<IActionResult> GetUltimaConstante(long idMision)
        {
            var constante = await _context.ConstantesVitales
                .Where(c => c.IdMision == idMision)
                .OrderByDescending(c => c.FechaRegistro)
                .Select(c => new ConstanteVitalDTO
                {
                    FechaRegistro = c.FechaRegistro,
                    FrecuenciaCardiaca = c.FrecuenciaCardiaca,
                    PresionArterialSistolica = c.PresionSistolica,
                    PresionArterialDiastolica = c.PresionDiastolica,
                    SaturacionOxigeno = c.SaturacionOxigeno,
                    Temperatura = c.Temperatura
                })
                .FirstOrDefaultAsync();

            if (constante == null)
                return NotFound($"No hay constantes vitales registradas para la misión {idMision}");

            return Ok(constante);
        }

        /// <summary>
        /// POST: api/ConstantesVitales
        /// Registra una nueva constante vital (usando DTO en body)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CrearConstante([FromBody] RegistrarConstanteRequest request)
        {
            // Validaciones
            if (request == null)
                return BadRequest("Datos de constante no proporcionados");

            if (request.IdMision <= 0)
                return BadRequest("ID de misión inválido");

            // Verificar si la misión existe
            var misionExiste = await _context.Misiones.AnyAsync(m => m.IdMision == request.IdMision);
            if (!misionExiste)
                return NotFound($"La misión con ID {request.IdMision} no existe");

            // Validar rangos fisiológicos básicos
            if (request.FrecuenciaCardiaca < 0 || request.FrecuenciaCardiaca > 300)
                return BadRequest("Frecuencia cardiaca fuera de rango válido (0-300)");

            if (request.SaturacionOxigeno < 0 || request.SaturacionOxigeno > 100)
                return BadRequest("Saturación de oxígeno fuera de rango válido (0-100)");

            if (request.Temperatura < 30 || request.Temperatura > 45)
                return BadRequest("Temperatura fuera de rango válido (30-45 °C)");

            if (request.PresionArterialSistolica < 50 || request.PresionArterialSistolica > 250)
                return BadRequest("Presión sistólica fuera de rango válido (50-250)");

            if (request.PresionArterialDiastolica < 30 || request.PresionArterialDiastolica > 150)
                return BadRequest("Presión diastólica fuera de rango válido (30-150)");

            var constante = new ConstanteVital
            {
                IdMision = request.IdMision,
                FechaRegistro = DateTime.UtcNow,  // 🔧 CORREGIDO: UTC para PostgreSQL
                FrecuenciaCardiaca = request.FrecuenciaCardiaca,
                PresionSistolica = request.PresionArterialSistolica,
                PresionDiastolica = request.PresionArterialDiastolica,
                SaturacionOxigeno = request.SaturacionOxigeno,
                FrecuenciaRespiratoria = request.FrecuenciaRespiratoria,
                Temperatura = request.Temperatura,
                Glasgow = request.Glasgow
            };

            _context.ConstantesVitales.Add(constante);
            await _context.SaveChangesAsync();

            // Determinar si es crítica
            var esCritica = EsConstanteCritica(constante);

            return Ok(new
            {
                mensaje = "Constante vital registrada correctamente",
                idConstante = constante.IdConstante,
                fechaRegistro = constante.FechaRegistro,
                esCritica = esCritica,
                recomendacion = esCritica ? "⚠️ PACIENTE CRÍTICO - NOTIFICAR A UCI INMEDIATAMENTE" : "Paciente estable - continuar monitoreo"
            });
        }

        /// <summary>
        /// POST: api/ConstantesVitales/registrar-multiples
        /// Registra múltiples constantes vitales de una vez
        /// </summary>
        [HttpPost("registrar-multiples")]
        public async Task<IActionResult> RegistrarMultiplesConstantes([FromBody] List<RegistrarConstanteRequest> requests)
        {
            if (requests == null || requests.Count == 0)
                return BadRequest("No se proporcionaron constantes vitales");

            var constantesRegistradas = new List<object>();
            var errores = new List<string>();

            foreach (var request in requests)
            {
                try
                {
                    var misionExiste = await _context.Misiones.AnyAsync(m => m.IdMision == request.IdMision);
                    if (!misionExiste)
                    {
                        errores.Add($"Misión {request.IdMision} no existe");
                        continue;
                    }

                    var constante = new ConstanteVital
                    {
                        IdMision = request.IdMision,
                        FechaRegistro = DateTime.UtcNow,
                        FrecuenciaCardiaca = request.FrecuenciaCardiaca,
                        PresionSistolica = request.PresionArterialSistolica,
                        PresionDiastolica = request.PresionArterialDiastolica,
                        SaturacionOxigeno = request.SaturacionOxigeno,
                        FrecuenciaRespiratoria = request.FrecuenciaRespiratoria,
                        Temperatura = request.Temperatura,
                        Glasgow = request.Glasgow
                    };

                    _context.ConstantesVitales.Add(constante);
                    constantesRegistradas.Add(new { request.IdMision, request.FrecuenciaCardiaca, request.SaturacionOxigeno });
                }
                catch (Exception ex)
                {
                    errores.Add($"Error en misión {request.IdMision}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = $"Se registraron {constantesRegistradas.Count} constantes vitales",
                registrosExitosos = constantesRegistradas.Count,
                errores = errores.Count > 0 ? errores : null
            });
        }

        // Métodos auxiliares
        private string ObtenerTendencia(List<ConstanteVitalDTO> constantes)
        {
            if (constantes.Count < 2)
                return "DATOS INSUFICIENTES";

            var ultima = constantes.First();
            var primera = constantes.Last();

            if (ultima.SaturacionOxigeno > primera.SaturacionOxigeno + 5)
                return "✅ MEJORANDO SIGNIFICATIVAMENTE";
            else if (ultima.SaturacionOxigeno > primera.SaturacionOxigeno)
                return "📈 MEJORANDO LIGERAMENTE";
            else if (ultima.SaturacionOxigeno < primera.SaturacionOxigeno - 5)
                return "⚠️ DETERIORO SIGNIFICATIVO - ALERTA";
            else if (ultima.SaturacionOxigeno < primera.SaturacionOxigeno)
                return "📉 DETERIORO LIGERO";
            else
                return "➡️ ESTABLE";
        }

        private bool EsConstanteCritica(ConstanteVital constante)
        {
            return constante.FrecuenciaCardiaca > 120 ||      // Taquicardia severa
                   constante.FrecuenciaCardiaca < 50 ||       // Bradicardia severa
                   constante.SaturacionOxigeno < 85 ||        // Hipoxia severa
                   constante.Temperatura > 39 ||              // Fiebre alta
                   constante.Temperatura < 35 ||              // Hipotermia
                   constante.PresionSistolica > 180 ||        // Crisis hipertensiva
                   constante.PresionSistolica < 80 ||         // Shock
                   (constante.Glasgow.HasValue && constante.Glasgow < 9);  // Coma
        }
    }

    // Request DTO para registrar constante vital
    public class RegistrarConstanteRequest
    {
        public long IdMision { get; set; }
        public int FrecuenciaCardiaca { get; set; }
        public int PresionArterialSistolica { get; set; }
        public int PresionArterialDiastolica { get; set; }
        public int SaturacionOxigeno { get; set; }
        public int FrecuenciaRespiratoria { get; set; }
        public decimal Temperatura { get; set; }
        public int? Glasgow { get; set; }
    }
}