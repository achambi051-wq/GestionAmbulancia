using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ambulancia_MIS.Data;
using Ambulancia_MIS.Dominio;
using Ambulancia_MIS.DTOs;

namespace Ambulancia_MIS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistorialUbicacionController : ControllerBase
    {
        private readonly AmbulanciaContext _context;

        public HistorialUbicacionController(AmbulanciaContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/HistorialUbicacion/mision/1
        /// Obtiene todo el historial de ubicaciones de una misión
        /// </summary>
        [HttpGet("mision/{idMision}")]
        public async Task<IActionResult> GetHistorialPorMision(long idMision)
        {
            // Verificar si la misión existe
            var misionExiste = await _context.Misiones.AnyAsync(m => m.IdMision == idMision);
            if (!misionExiste)
                return NotFound($"La misión con ID {idMision} no existe");

            var historial = await _context.HistorialUbicaciones
                .Where(h => h.IdMision == idMision)
                .OrderBy(h => h.FechaHora)
                .Select(h => new HistorialUbicacionDTO
                {
                    FechaHora = h.FechaHora,
                    Latitud = h.Latitud,
                    Longitud = h.Longitud,
                    VelocidadKmh = h.VelocidadKmh
                })
                .ToListAsync();

            // Calcular estadísticas de ruta
            var distanciaTotal = CalcularDistanciaTotal(historial);
            var velocidadPromedio = historial.Any() ? historial.Average(h => h.VelocidadKmh) : 0;
            var velocidadMaxima = historial.Any() ? historial.Max(h => h.VelocidadKmh) : 0;

            return Ok(new
            {
                IdMision = idMision,
                TotalRegistros = historial.Count,
                FechaInicio = historial.FirstOrDefault()?.FechaHora,
                FechaFin = historial.LastOrDefault()?.FechaHora,
                DistanciaTotalKm = Math.Round(distanciaTotal, 2),
                VelocidadPromedioKmh = Math.Round(velocidadPromedio, 1),
                VelocidadMaximaKmh = velocidadMaxima,
                Historial = historial
            });
        }

        /// <summary>
        /// GET: api/HistorialUbicacion/ambulancia/{idAmbulancia}
        /// Obtiene el historial de ubicaciones de una ambulancia específica
        /// </summary>
        [HttpGet("ambulancia/{idAmbulancia}")]
        public async Task<IActionResult> GetHistorialPorAmbulancia(long idAmbulancia, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var query = _context.HistorialUbicaciones
                .Where(h => h.IdAmbulancia == idAmbulancia);

            if (desde.HasValue)
                query = query.Where(h => h.FechaHora >= desde.Value);
            if (hasta.HasValue)
                query = query.Where(h => h.FechaHora <= hasta.Value);

            var historial = await query
                .OrderByDescending(h => h.FechaHora)
                .Select(h => new HistorialUbicacionDTO
                {
                    FechaHora = h.FechaHora,
                    Latitud = h.Latitud,
                    Longitud = h.Longitud,
                    VelocidadKmh = h.VelocidadKmh
                })
                .ToListAsync();

            return Ok(new
            {
                IdAmbulancia = idAmbulancia,
                Periodo = new { Desde = desde, Hasta = hasta },
                TotalRegistros = historial.Count,
                Historial = historial
            });
        }

        /// <summary>
        /// GET: api/HistorialUbicacion/ultima/{idMision}
        /// Obtiene la última ubicación registrada de una misión
        /// </summary>
        [HttpGet("ultima/{idMision}")]
        public async Task<IActionResult> GetUltimaUbicacion(long idMision)
        {
            var ubicacion = await _context.HistorialUbicaciones
                .Where(h => h.IdMision == idMision)
                .OrderByDescending(h => h.FechaHora)
                .Select(h => new HistorialUbicacionDTO
                {
                    FechaHora = h.FechaHora,
                    Latitud = h.Latitud,
                    Longitud = h.Longitud,
                    VelocidadKmh = h.VelocidadKmh
                })
                .FirstOrDefaultAsync();

            if (ubicacion == null)
                return NotFound($"No hay ubicaciones registradas para la misión {idMision}");

            return Ok(ubicacion);
        }

        /// <summary>
        /// POST: api/HistorialUbicacion
        /// Registra una nueva ubicación
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RegistrarUbicacion([FromBody] RegistrarUbicacionRequest request)
        {
            // Validaciones
            if (request == null)
                return BadRequest("Datos de ubicación no proporcionados");

            if (request.IdMision <= 0)
                return BadRequest("ID de misión inválido");

            if (request.IdAmbulancia <= 0)
                return BadRequest("ID de ambulancia inválido");

            // Verificar si la misión existe
            var misionExiste = await _context.Misiones.AnyAsync(m => m.IdMision == request.IdMision);
            if (!misionExiste)
                return NotFound($"La misión con ID {request.IdMision} no existe");

            // Verificar si la ambulancia existe
            var ambulanciaExiste = await _context.Ambulancias.AnyAsync(a => a.IdAmbulancia == request.IdAmbulancia);
            if (!ambulanciaExiste)
                return NotFound($"La ambulancia con ID {request.IdAmbulancia} no existe");

            // Validar coordenadas
            if (request.Latitud < -90 || request.Latitud > 90)
                return BadRequest("Latitud fuera de rango (-90 a 90)");

            if (request.Longitud < -180 || request.Longitud > 180)
                return BadRequest("Longitud fuera de rango (-180 a 180)");

            if (request.VelocidadKmh < 0 || request.VelocidadKmh > 200)
                return BadRequest("Velocidad fuera de rango (0-200 km/h)");

            var ubicacion = new HistorialUbicacion
            {
                IdMision = request.IdMision,
                IdAmbulancia = request.IdAmbulancia,
                FechaHora = DateTime.UtcNow,  // 🔧 CORREGIDO: UTC para PostgreSQL
                Latitud = request.Latitud,
                Longitud = request.Longitud,
                VelocidadKmh = request.VelocidadKmh
            };

            _context.HistorialUbicaciones.Add(ubicacion);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Ubicación registrada correctamente",
                idHistorial = ubicacion.IdHistorial,
                fecha = ubicacion.FechaHora,
                latitud = ubicacion.Latitud,
                longitud = ubicacion.Longitud
            });
        }

        /// <summary>
        /// POST: api/HistorialUbicacion/registrar-ruta
        /// Registra múltiples ubicaciones (ruta completa)
        /// </summary>
        [HttpPost("registrar-ruta")]
        public async Task<IActionResult> RegistrarRuta([FromBody] List<RegistrarUbicacionRequest> requests)
        {
            if (requests == null || requests.Count == 0)
                return BadRequest("No se proporcionaron ubicaciones");

            var ubicacionesRegistradas = new List<object>();
            var errores = new List<string>();

            foreach (var request in requests)
            {
                try
                {
                    var ubicacion = new HistorialUbicacion
                    {
                        IdMision = request.IdMision,
                        IdAmbulancia = request.IdAmbulancia,
                        FechaHora = DateTime.UtcNow,
                        Latitud = request.Latitud,
                        Longitud = request.Longitud,
                        VelocidadKmh = request.VelocidadKmh
                    };

                    _context.HistorialUbicaciones.Add(ubicacion);
                    ubicacionesRegistradas.Add(new { request.IdMision, request.Latitud, request.Longitud });
                }
                catch (Exception ex)
                {
                    errores.Add($"Error en punto {ubicacionesRegistradas.Count + 1}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = $"Se registraron {ubicacionesRegistradas.Count} ubicaciones",
                puntosRegistrados = ubicacionesRegistradas.Count,
                errores = errores.Count > 0 ? errores : null
            });
        }

        /// <summary>
        /// DELETE: api/HistorialUbicacion/mision/{idMision}
        /// Elimina todo el historial de ubicaciones de una misión
        /// </summary>
        [HttpDelete("mision/{idMision}")]
        public async Task<IActionResult> EliminarHistorialMision(long idMision)
        {
            var historial = await _context.HistorialUbicaciones
                .Where(h => h.IdMision == idMision)
                .ToListAsync();

            if (historial.Count == 0)
                return NotFound($"No hay historial de ubicaciones para la misión {idMision}");

            _context.HistorialUbicaciones.RemoveRange(historial);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = $"Se eliminaron {historial.Count} registros de ubicación",
                idMision,
                registrosEliminados = historial.Count
            });
        }

        // Método auxiliar para calcular distancia total de la ruta (fórmula de Haversine)
        private double CalcularDistanciaTotal(List<HistorialUbicacionDTO> historial)
        {
            if (historial.Count < 2) return 0;

            double distanciaTotal = 0;
            const double R = 6371; // Radio de la Tierra en km

            for (int i = 1; i < historial.Count; i++)
            {
                var p1 = historial[i - 1];
                var p2 = historial[i];

                var lat1 = ConvertToRadians((double)p1.Latitud);
                var lon1 = ConvertToRadians((double)p1.Longitud);
                var lat2 = ConvertToRadians((double)p2.Latitud);
                var lon2 = ConvertToRadians((double)p2.Longitud);

                var dlat = lat2 - lat1;
                var dlon = lon2 - lon1;

                var a = Math.Sin(dlat / 2) * Math.Sin(dlat / 2) +
                        Math.Cos(lat1) * Math.Cos(lat2) *
                        Math.Sin(dlon / 2) * Math.Sin(dlon / 2);

                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                distanciaTotal += R * c;
            }

            return distanciaTotal;
        }

        private double ConvertToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }

    // Request DTO para registrar ubicación
    public class RegistrarUbicacionRequest
    {
        public long IdMision { get; set; }
        public long IdAmbulancia { get; set; }
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public int VelocidadKmh { get; set; }
    }
}