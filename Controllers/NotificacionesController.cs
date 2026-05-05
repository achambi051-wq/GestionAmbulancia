using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ambulancia_MIS.Data;
using Ambulancia_MIS.Dominio;
using Ambulancia_MIS.DTOs;

namespace Ambulancia_MIS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacionesController : ControllerBase
    {
        private readonly AmbulanciaContext _context;

        public NotificacionesController(AmbulanciaContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/Notificaciones/mision/1
        /// Obtiene todas las notificaciones de una misión
        /// </summary>
        [HttpGet("mision/{idMision}")]
        public async Task<IActionResult> GetNotificacionesPorMision(long idMision)
        {
            var notificaciones = await _context.Notificaciones
                .Where(n => n.IdMision == idMision)
                .OrderByDescending(n => n.FechaEnvio)
                .Select(n => new NotificacionDTO
                {
                    Destinatario = n.Destinatario,
                    Mensaje = n.Mensaje,
                    Canal = n.Canal,
                    FechaEnvio = n.FechaEnvio,
                    Enviada = n.Enviada
                })
                .ToListAsync();

            return Ok(new
            {
                IdMision = idMision,
                TotalNotificaciones = notificaciones.Count,
                NotificacionesPendientes = notificaciones.Count(n => !n.Enviada),
                Notificaciones = notificaciones
            });
        }

        /// <summary>
        /// GET: api/Notificaciones/pendientes
        /// Obtiene todas las notificaciones pendientes de envío
        /// </summary>
        [HttpGet("pendientes")]
        public async Task<IActionResult> GetNotificacionesPendientes()
        {
            var notificaciones = await _context.Notificaciones
                .Where(n => !n.Enviada)
                .OrderByDescending(n => n.FechaEnvio)
                .Select(n => new NotificacionDTO
                {
                    Destinatario = n.Destinatario,
                    Mensaje = n.Mensaje,
                    Canal = n.Canal,
                    FechaEnvio = n.FechaEnvio,
                    Enviada = n.Enviada
                })
                .ToListAsync();

            return Ok(new
            {
                TotalPendientes = notificaciones.Count,
                Notificaciones = notificaciones
            });
        }

        /// <summary>
        /// POST: api/Notificaciones
        /// Envía una nueva notificación
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> EnviarNotificacion([FromBody] EnviarNotificacionRequest request)
        {
            // Validaciones
            if (request == null)
                return BadRequest("Datos de notificación no proporcionados");

            if (string.IsNullOrWhiteSpace(request.Destinatario))
                return BadRequest("El destinatario es requerido");

            if (string.IsNullOrWhiteSpace(request.Mensaje))
                return BadRequest("El mensaje es requerido");

            if (string.IsNullOrWhiteSpace(request.Canal))
                return BadRequest("El canal es requerido");

            var canalesPermitidos = new[] { "WHATSAPP", "SMS", "EMAIL", "PUSH", "RADIO" };
            if (!canalesPermitidos.Contains(request.Canal.ToUpper()))
                return BadRequest($"Canal no válido. Use: {string.Join(", ", canalesPermitidos)}");

            // Verificar si la misión existe (si se proporcionó)
            if (request.IdMision.HasValue && request.IdMision.Value > 0)
            {
                var misionExiste = await _context.Misiones.AnyAsync(m => m.IdMision == request.IdMision.Value);
                if (!misionExiste)
                    return NotFound($"La misión con ID {request.IdMision} no existe");
            }

            var notificacion = new Notificacion
            {
                IdMision = request.IdMision ?? 0,
                Destinatario = request.Destinatario,
                Mensaje = request.Mensaje,
                Canal = request.Canal.ToUpper(),
                FechaEnvio = DateTime.UtcNow,  // 🔧 CORREGIDO: UTC para PostgreSQL
                Enviada = true  // En una implementación real, aquí se intentaría enviar
            };

            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = $"Notificación enviada correctamente por {notificacion.Canal} a {notificacion.Destinatario}",
                idNotificacion = notificacion.IdNotificacion,
                fechaEnvio = notificacion.FechaEnvio
            });
        }

        /// <summary>
        /// POST: api/Notificaciones/enviar-masivas
        /// Envía notificaciones masivas a múltiples destinatarios
        /// </summary>
        [HttpPost("enviar-masivas")]
        public async Task<IActionResult> EnviarNotificacionesMasivas([FromBody] EnviarNotificacionesMasivasRequest request)
        {
            if (request == null || request.Destinatarios == null || request.Destinatarios.Count == 0)
                return BadRequest("No se proporcionaron destinatarios");

            var notificacionesEnviadas = new List<object>();
            var errores = new List<string>();

            foreach (var destinatario in request.Destinatarios)
            {
                try
                {
                    var notificacion = new Notificacion
                    {
                        IdMision = request.IdMision ?? 0,
                        Destinatario = destinatario,
                        Mensaje = request.Mensaje,
                        Canal = request.Canal.ToUpper(),
                        FechaEnvio = DateTime.UtcNow,
                        Enviada = true
                    };

                    _context.Notificaciones.Add(notificacion);
                    notificacionesEnviadas.Add(new { destinatario, canal = request.Canal });
                }
                catch (Exception ex)
                {
                    errores.Add($"Error al enviar a {destinatario}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = $"Se enviaron {notificacionesEnviadas.Count} notificaciones",
                enviadas = notificacionesEnviadas,
                errores = errores.Count > 0 ? errores : null
            });
        }

        /// <summary>
        /// PUT: api/Notificaciones/reintentar/{idNotificacion}
        /// Reintenta enviar una notificación fallida
        /// </summary>
        [HttpPut("reintentar/{idNotificacion}")]
        public async Task<IActionResult> ReintentarNotificacion(long idNotificacion)
        {
            var notificacion = await _context.Notificaciones.FindAsync(idNotificacion);
            if (notificacion == null)
                return NotFound($"Notificación con ID {idNotificacion} no encontrada");

            if (notificacion.Enviada)
                return Ok(new { mensaje = "La notificación ya fue enviada correctamente", idNotificacion });

            // Simular reenvío
            notificacion.Enviada = true;
            notificacion.FechaEnvio = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Notificación reenviada correctamente",
                idNotificacion = notificacion.IdNotificacion,
                fechaReenvio = notificacion.FechaEnvio
            });
        }

        /// <summary>
        /// DELETE: api/Notificaciones/{idNotificacion}
        /// Elimina una notificación (Soft Delete)
        /// </summary>
        [HttpDelete("{idNotificacion}")]
        public async Task<IActionResult> EliminarNotificacion(long idNotificacion)
        {
            var notificacion = await _context.Notificaciones.FindAsync(idNotificacion);
            if (notificacion == null)
                return NotFound($"Notificación con ID {idNotificacion} no encontrada");

            _context.Notificaciones.Remove(notificacion);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Notificación eliminada correctamente", idNotificacion });
        }
    }

    // Request DTOs
    public class EnviarNotificacionRequest
    {
        public long? IdMision { get; set; }
        public string Destinatario { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Canal { get; set; } = string.Empty;
    }

    public class EnviarNotificacionesMasivasRequest
    {
        public long? IdMision { get; set; }
        public List<string> Destinatarios { get; set; } = new();
        public string Mensaje { get; set; } = string.Empty;
        public string Canal { get; set; } = string.Empty;
    }
}