using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ambulancia_MIS.Data;
using Ambulancia_MIS.DTOs;

namespace Ambulancia_MIS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmbulanciaInsumoController : ControllerBase
    {
        private readonly AmbulanciaContext _context;

        public AmbulanciaInsumoController(AmbulanciaContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET para LOGÍSTICA: Situación actual de todas las ambulancias
        /// </summary>
        [HttpGet("situacion-actual")]
        public async Task<IActionResult> GetSituacionActual()
        {
            var plantilla = await (from p in _context.PlantillaInsumos
                                   join i in _context.Insumos on p.IdInsumo equals i.IdInsumo
                                   where i.Estado == "ACTIVO"
                                   select new
                                   {
                                       i.Codigo,
                                       i.Nombre,
                                       i.UnidadMedida,
                                       p.CantidadRequerida,
                                       p.NivelCritico
                                   }).ToListAsync();

            var ambulancias = await _context.Ambulancias
                .Where(a => a.Estado == "ACTIVO")
                .ToListAsync();

            var resultados = new List<SituacionActualDTO>();

            foreach (var ambulancia in ambulancias)
            {
                var stockActual = await (from ai in _context.AmbulanciaInsumos
                                         join i in _context.Insumos on ai.IdInsumo equals i.IdInsumo
                                         where ai.IdAmbulancia == ambulancia.IdAmbulancia && ai.Estado == "ACTIVO"
                                         select new { i.Codigo, CantidadActual = ai.CantidadActual })
                                        .ToDictionaryAsync(x => x.Codigo, x => x.CantidadActual);

                var insumosSituacion = new List<InsumoSituacionDTO>();
                var totalFaltantes = 0;
                var totalCriticos = 0;

                foreach (var req in plantilla)
                {
                    var cantidadActual = stockActual.GetValueOrDefault(req.Codigo, 0);
                    var diferencia = req.CantidadRequerida - cantidadActual;
                    var esCritico = diferencia > 0 && cantidadActual <= req.NivelCritico;

                    if (diferencia > 0) totalFaltantes += diferencia;
                    if (esCritico) totalCriticos++;

                    insumosSituacion.Add(new InsumoSituacionDTO
                    {
                        CodigoInsumo = req.Codigo,
                        NombreInsumo = req.Nombre,
                        UnidadMedida = req.UnidadMedida,
                        CantidadRequerida = req.CantidadRequerida,
                        CantidadActual = cantidadActual,
                        Diferencia = diferencia,
                        Estado = diferencia > 0 ? "FALTANTE" : diferencia < 0 ? "SOBRANTE" : "OK",
                        EsCritico = esCritico,
                        CantidadAReponer = diferencia > 0 ? diferencia : 0
                    });
                }

                resultados.Add(new SituacionActualDTO
                {
                    CodigoAmbulancia = ambulancia.Codigo,
                    TipoAmbulancia = ambulancia.Tipo,
                    Estado = ambulancia.Estado,
                    FechaConsulta = DateTime.UtcNow,
                    Insumos = insumosSituacion,
                    Resumen = new ResumenSituacionDTO
                    {
                        TotalInsumosRequeridos = plantilla.Count,
                        TotalInsumosActuales = insumosSituacion.Sum(x => x.CantidadActual),
                        TotalFaltantes = totalFaltantes,
                        TotalCriticos = totalCriticos,
                        RequiereReposicionUrgente = totalFaltantes > 0,
                        Mensaje = totalFaltantes > 0
                            ? $"⚠️ REQUIERE REPOSICIÓN: Faltan {totalFaltantes} unidades en {totalCriticos} insumos críticos"
                            : "✓ TODO EN REGLA - Stock suficiente"
                    }
                });
            }

            return Ok(resultados);
        }

        /// <summary>
        /// GET específico para una sola ambulancia
        /// </summary>
        [HttpGet("situacion-actual/{codigoAmbulancia}")]
        public async Task<IActionResult> GetSituacionActualPorAmbulancia(string codigoAmbulancia)
        {
            var ambulancia = await _context.Ambulancias
                .FirstOrDefaultAsync(a => a.Codigo == codigoAmbulancia && a.Estado == "ACTIVO");

            if (ambulancia == null) return NotFound();

            var plantilla = await (from p in _context.PlantillaInsumos
                                   join i in _context.Insumos on p.IdInsumo equals i.IdInsumo
                                   where i.Estado == "ACTIVO"
                                   select new
                                   {
                                       i.Codigo,
                                       i.Nombre,
                                       i.UnidadMedida,
                                       p.CantidadRequerida,
                                       p.NivelCritico
                                   }).ToListAsync();

            var stockActual = await (from ai in _context.AmbulanciaInsumos
                                     join i in _context.Insumos on ai.IdInsumo equals i.IdInsumo
                                     where ai.IdAmbulancia == ambulancia.IdAmbulancia && ai.Estado == "ACTIVO"
                                     select new { i.Codigo, CantidadActual = ai.CantidadActual })
                                    .ToDictionaryAsync(x => x.Codigo, x => x.CantidadActual);

            var insumosSituacion = new List<InsumoSituacionDTO>();
            var totalFaltantes = 0;
            var totalCriticos = 0;

            foreach (var req in plantilla)
            {
                var cantidadActual = stockActual.GetValueOrDefault(req.Codigo, 0);
                var diferencia = req.CantidadRequerida - cantidadActual;
                var esCritico = diferencia > 0 && cantidadActual <= req.NivelCritico;

                if (diferencia > 0) totalFaltantes += diferencia;
                if (esCritico) totalCriticos++;

                insumosSituacion.Add(new InsumoSituacionDTO
                {
                    CodigoInsumo = req.Codigo,
                    NombreInsumo = req.Nombre,
                    UnidadMedida = req.UnidadMedida,
                    CantidadRequerida = req.CantidadRequerida,
                    CantidadActual = cantidadActual,
                    Diferencia = diferencia,
                    Estado = diferencia > 0 ? "FALTANTE" : diferencia < 0 ? "SOBRANTE" : "OK",
                    EsCritico = esCritico,
                    CantidadAReponer = diferencia > 0 ? diferencia : 0
                });
            }

            var resultado = new SituacionActualDTO
            {
                CodigoAmbulancia = ambulancia.Codigo,
                TipoAmbulancia = ambulancia.Tipo,
                Estado = ambulancia.Estado,
                FechaConsulta = DateTime.UtcNow,
                Insumos = insumosSituacion,
                Resumen = new ResumenSituacionDTO
                {
                    TotalInsumosRequeridos = plantilla.Count,
                    TotalInsumosActuales = insumosSituacion.Sum(x => x.CantidadActual),
                    TotalFaltantes = totalFaltantes,
                    TotalCriticos = totalCriticos,
                    RequiereReposicionUrgente = totalFaltantes > 0,
                    Mensaje = totalFaltantes > 0
                        ? $"⚠️ REQUIERE REPOSICIÓN: Faltan {totalFaltantes} unidades en {totalCriticos} insumos críticos"
                        : "✓ TODO EN REGLA - Stock suficiente"
                }
            };

            return Ok(resultado);
        }

        /// <summary>
        /// Registrar consumo de insumos después de una misión
        /// </summary>
        [HttpPost("registrar-consumo")]
        public async Task<IActionResult> RegistrarConsumo(string codigoAmbulancia, List<ConsumoRegistroDTO> consumos)
        {
            var ambulancia = await _context.Ambulancias
                .FirstOrDefaultAsync(a => a.Codigo == codigoAmbulancia);

            if (ambulancia == null)
                return BadRequest("Ambulancia no encontrada");

            foreach (var consumo in consumos)
            {
                var insumo = await _context.Insumos
                    .FirstOrDefaultAsync(i => i.Codigo == consumo.CodigoInsumo);

                if (insumo == null) continue;

                var relacion = await _context.AmbulanciaInsumos
                    .FirstOrDefaultAsync(ai => ai.IdAmbulancia == ambulancia.IdAmbulancia
                                            && ai.IdInsumo == insumo.IdInsumo);

                if (relacion != null)
                {
                    relacion.CantidadActual -= consumo.Cantidad;
                    relacion.FechaUltimaReposicion = DateTime.UtcNow;
                    _context.AmbulanciaInsumos.Update(relacion);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Consumo registrado correctamente" });
        }
    }

    public class ConsumoRegistroDTO
    {
        public string CodigoInsumo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }
}