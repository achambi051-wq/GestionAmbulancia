using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ambulancia_MIS.Data;
using Ambulancia_MIS.Dominio;
using Ambulancia_MIS.DTOs;

namespace Ambulancia_MIS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertasCriticasController : ControllerBase
    {
        private readonly AmbulanciaContext _context;

        public AlertasCriticasController(AmbulanciaContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/AlertasCriticas/mision/1
        /// Obtiene todas las alertas de una misión específica
        /// </summary>
        [HttpGet("mision/{idMision}")]
        public async Task<IActionResult> GetAlertasPorMision(long idMision)
        {
            // Verificar si la misión existe
            var misionExiste = await _context.Misiones.AnyAsync(m => m.IdMision == idMision);
            if (!misionExiste)
                return NotFound($"La misión con ID {idMision} no existe");

            var alertas = await _context.AlertasCriticas
                .Where(a => a.IdMision == idMision)
                .OrderByDescending(a => a.FechaAlerta)
                .Select(a => new AlertaCriticaDTO
                {
                    Tipo = a.Tipo,
                    Gravedad = a.Gravedad,
                    FechaAlerta = a.FechaAlerta,
                    Atendida = a.Atendida
                })
                .ToListAsync();

            var resumen = new
            {
                IdMision = idMision,
                TotalAlertas = alertas.Count,
                AlertasPendientes = alertas.Count(a => !a.Atendida),
                AlertasAtendidas = alertas.Count(a => a.Atendida),
                Alertas = alertas
            };

            return Ok(resumen);
        }

        /// <summary>
        /// GET: api/AlertasCriticas/pendientes
        /// Obtiene todas las alertas no atendidas del sistema
        /// </summary>
        [HttpGet("pendientes")]
        public async Task<IActionResult> GetAlertasPendientes()
        {
            var alertas = await _context.AlertasCriticas
                .Where(a => !a.Atendida)
                .OrderByDescending(a => a.FechaAlerta)
                .Select(a => new AlertaCriticaDTO
                {
                    Tipo = a.Tipo,
                    Gravedad = a.Gravedad,
                    FechaAlerta = a.FechaAlerta,
                    Atendida = a.Atendida
                })
                .ToListAsync();

            return Ok(new
            {
                TotalPendientes = alertas.Count,
                Alertas = alertas
            });
        }

        /// <summary>
        /// GET: api/AlertasCriticas/gravedad/{gravedad}
        /// Obtiene alertas filtradas por nivel de gravedad (ALTA, MEDIA, BAJA)
        /// </summary>
        [HttpGet("gravedad/{gravedad}")]
        public async Task<IActionResult> GetAlertasPorGravedad(string gravedad)
        {
            var gravedadUpper = gravedad.ToUpper();
            var validGravedades = new[] { "ALTA", "MEDIA", "BAJA", "CRITICA" };

            if (!validGravedades.Contains(gravedadUpper))
                return BadRequest($"Gravedad no válida. Use: {string.Join(", ", validGravedades)}");

            var alertas = await _context.AlertasCriticas
                .Where(a => a.Gravedad == gravedadUpper && !a.Atendida)
                .OrderByDescending(a => a.FechaAlerta)
                .Select(a => new AlertaCriticaDTO
                {
                    Tipo = a.Tipo,
                    Gravedad = a.Gravedad,
                    FechaAlerta = a.FechaAlerta,
                    Atendida = a.Atendida
                })
                .ToListAsync();

            return Ok(new
            {
                Gravedad = gravedadUpper,
                TotalAlertas = alertas.Count,
                Alertas = alertas
            });
        }

        /// <summary>
        /// POST: api/AlertasCriticas
        /// Crea una nueva alerta crítica para una misión
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CrearAlerta([FromBody] CrearAlertaRequest request)
        {
            // Validaciones
            if (request == null)
                return BadRequest("Datos de alerta no proporcionados");

            if (request.IdMision <= 0)
                return BadRequest("ID de misión inválido");

            if (string.IsNullOrWhiteSpace(request.Tipo))
                return BadRequest("El tipo de alerta es requerido");

            if (string.IsNullOrWhiteSpace(request.Gravedad))
                return BadRequest("La gravedad es requerida");

            // Verificar si la misión existe
            var misionExiste = await _context.Misiones.AnyAsync(m => m.IdMision == request.IdMision);
            if (!misionExiste)
                return NotFound($"La misión con ID {request.IdMision} no existe");

            // Validar valores permitidos
            var tiposPermitidos = new[] { "UCI", "PARO_CARDIACO", "HEMORRAGIA", "TRAUMA", "EPIDEMIA", "FALLA_EQUIPO" };
            var gravedadesPermitidas = new[] { "BAJA", "MEDIA", "ALTA", "CRITICA" };

            if (!tiposPermitidos.Contains(request.Tipo.ToUpper()))
                return BadRequest($"Tipo no válido. Use: {string.Join(", ", tiposPermitidos)}");

            if (!gravedadesPermitidas.Contains(request.Gravedad.ToUpper()))
                return BadRequest($"Gravedad no válida. Use: {string.Join(", ", gravedadesPermitidas)}");

            // Crear la alerta
            var alerta = new AlertaCritica
            {
                IdMision = request.IdMision,
                Tipo = request.Tipo.ToUpper(),
                Gravedad = request.Gravedad.ToUpper(),
                FechaAlerta = DateTime.UtcNow,  // 🔧 CORREGIDO: UTC para PostgreSQL
                Atendida = false
            };

            _context.AlertasCriticas.Add(alerta);
            await _context.SaveChangesAsync();

            // Obtener información de la misión para contexto
            var mision = await _context.Misiones
                .Where(m => m.IdMision == request.IdMision)
                .Select(m => new { m.NumeroSolicitud, m.Prioridad })
                .FirstOrDefaultAsync();

            return Ok(new
            {
                mensaje = "Alerta crítica generada correctamente",
                idAlerta = alerta.IdAlerta,
                tipo = alerta.Tipo,
                gravedad = alerta.Gravedad,
                fechaAlerta = alerta.FechaAlerta,
                mision = mision?.NumeroSolicitud,
                prioridadMision = mision?.Prioridad,
                recomendacion = ObtenerRecomendacionPorGravedad(alerta.Gravedad)
            });
        }

        /// <summary>
        /// POST: api/AlertasCriticas/crear-varias
        /// Crea múltiples alertas de una vez
        /// </summary>
        [HttpPost("crear-varias")]
        public async Task<IActionResult> CrearMultiplesAlertas([FromBody] List<CrearAlertaRequest> requests)
        {
            if (requests == null || requests.Count == 0)
                return BadRequest("No se proporcionaron alertas");

            var alertasCreadas = new List<object>();
            var errores = new List<string>();

            foreach (var request in requests)
            {
                try
                {
                    // Verificar si la misión existe
                    var misionExiste = await _context.Misiones.AnyAsync(m => m.IdMision == request.IdMision);
                    if (!misionExiste)
                    {
                        errores.Add($"Misión {request.IdMision} no existe");
                        continue;
                    }

                    var alerta = new AlertaCritica
                    {
                        IdMision = request.IdMision,
                        Tipo = request.Tipo.ToUpper(),
                        Gravedad = request.Gravedad.ToUpper(),
                        FechaAlerta = DateTime.UtcNow,
                        Atendida = false
                    };

                    _context.AlertasCriticas.Add(alerta);
                    alertasCreadas.Add(new { request.IdMision, request.Tipo, request.Gravedad });
                }
                catch (Exception ex)
                {
                    errores.Add($"Error al crear alerta para misión {request.IdMision}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = $"Se crearon {alertasCreadas.Count} alertas",
                alertasCreadas = alertasCreadas,
                errores = errores.Count > 0 ? errores : null
            });
        }

        /// <summary>
        /// PUT: api/AlertasCriticas/marcar-atendida/{idAlerta}
        /// Marca una alerta como atendida
        /// </summary>
        [HttpPut("marcar-atendida/{idAlerta}")]
        public async Task<IActionResult> MarcarAlertaAtendida(long idAlerta)
        {
            var alerta = await _context.AlertasCriticas.FindAsync(idAlerta);
            if (alerta == null)
                return NotFound($"Alerta con ID {idAlerta} no encontrada");

            if (alerta.Atendida)
                return Ok(new { mensaje = "La alerta ya estaba marcada como atendida", idAlerta });

            alerta.Atendida = true;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Alerta marcada como atendida",
                idAlerta = alerta.IdAlerta,
                fechaAtencion = DateTime.UtcNow
            });
        }

        /// <summary>
        /// PUT: api/AlertasCriticas/marcar-todas-atendidas/{idMision}
        /// Marca todas las alertas de una misión como atendidas
        /// </summary>
        [HttpPut("marcar-todas-atendidas/{idMision}")]
        public async Task<IActionResult> MarcarTodasAlertasAtendidas(long idMision)
        {
            // Verificar si la misión existe
            var misionExiste = await _context.Misiones.AnyAsync(m => m.IdMision == idMision);
            if (!misionExiste)
                return NotFound($"La misión con ID {idMision} no existe");

            var alertas = await _context.AlertasCriticas
                .Where(a => a.IdMision == idMision && !a.Atendida)
                .ToListAsync();

            if (alertas.Count == 0)
                return Ok(new { mensaje = "No hay alertas pendientes para esta misión", idMision });

            foreach (var alerta in alertas)
            {
                alerta.Atendida = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = $"Se marcaron {alertas.Count} alertas como atendidas",
                idMision,
                totalAlertas = alertas.Count
            });
        }

        /// <summary>
        /// DELETE: api/AlertasCriticas/{idAlerta}
        /// Elimina una alerta (solo si está atendida o es muy antigua)
        /// </summary>
        [HttpDelete("{idAlerta}")]
        public async Task<IActionResult> EliminarAlerta(long idAlerta)
        {
            var alerta = await _context.AlertasCriticas.FindAsync(idAlerta);
            if (alerta == null)
                return NotFound($"Alerta con ID {idAlerta} no encontrada");

            // No permitir eliminar alertas no atendidas recientes (menos de 24 horas)
            var horasTranscurridas = (DateTime.UtcNow - alerta.FechaAlerta).TotalHours;
            if (!alerta.Atendida && horasTranscurridas < 24)
                return BadRequest("No se puede eliminar una alerta no atendida reciente. Márquela como atendida primero.");

            _context.AlertasCriticas.Remove(alerta);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Alerta eliminada correctamente", idAlerta });
        }

        /// <summary>
        /// GET: api/AlertasCriticas/resumen
        /// Obtiene un resumen estadístico de alertas
        /// </summary>
        [HttpGet("resumen")]
        public async Task<IActionResult> GetResumenAlertas()
        {
            var totalAlertas = await _context.AlertasCriticas.CountAsync();
            var alertasPendientes = await _context.AlertasCriticas.CountAsync(a => !a.Atendida);
            var alertasAtendidas = await _context.AlertasCriticas.CountAsync(a => a.Atendida);

            var alertasPorGravedad = await _context.AlertasCriticas
                .GroupBy(a => a.Gravedad)
                .Select(g => new { Gravedad = g.Key, Total = g.Count() })
                .ToListAsync();

            var alertasPorTipo = await _context.AlertasCriticas
                .GroupBy(a => a.Tipo)
                .Select(g => new { Tipo = g.Key, Total = g.Count() })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                TotalAlertas = totalAlertas,
                AlertasPendientes = alertasPendientes,
                AlertasAtendidas = alertasAtendidas,
                TasaAtencion = totalAlertas > 0 ? (alertasAtendidas * 100 / totalAlertas) : 0,
                AlertasPorGravedad = alertasPorGravedad,
                TopTiposDeAlerta = alertasPorTipo,
                FechaResumen = DateTime.UtcNow
            });
        }

        // Método auxiliar para recomendaciones según gravedad
        private string ObtenerRecomendacionPorGravedad(string gravedad)
        {
            return gravedad switch
            {
                "CRITICA" => "⚠️ ACTIVAR PROTOCOLO DE EMERGENCIA MÁXIMA - NOTIFICAR A DIRECCIÓN",
                "ALTA" => "⚠️ ACTIVAR PROTOCOLO URGENTE - COORDINAR CON UCI",
                "MEDIA" => "📋 ATENDER EN PRÓXIMAS 2 HORAS - NOTIFICAR A SUPERVISOR",
                "BAJA" => "📝 REGISTRAR EN SISTEMA - ATENDER EN TURNO",
                _ => "REVISAR PROTOCOLO ESTÁNDAR"
            };
        }
    }

    // Request DTO para crear alerta
    public class CrearAlertaRequest
    {
        public long IdMision { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Gravedad { get; set; } = string.Empty;
    }
}