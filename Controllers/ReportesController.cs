using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ambulancia_MIS.Data;

namespace Ambulancia_MIS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly AmbulanciaContext _context;

        public ReportesController(AmbulanciaContext context)
        {
            _context = context;
        }

        // ================================================
        // ✅ CONSULTA 1: JOIN entre 2 tablas (Ambulancia + AmbulanciaInsumo)
        // ================================================
        [HttpGet("ambulancias-con-stock")]
        public async Task<IActionResult> GetAmbulanciasConStock()
        {
            var resultado = await (from a in _context.Ambulancias
                                   join ai in _context.AmbulanciaInsumos on a.IdAmbulancia equals ai.IdAmbulancia
                                   where a.Estado == "ACTIVO" && ai.Estado == "ACTIVO"
                                   group new { a, ai } by new { a.Codigo, a.Placa, a.Tipo } into g
                                   select new
                                   {
                                       Ambulancia = g.Key.Codigo,
                                       Placa = g.Key.Placa,
                                       Tipo = g.Key.Tipo,
                                       TotalInsumos = g.Count(),
                                       StockTotal = g.Sum(x => x.ai.CantidadActual),
                                       EstadoGeneral = g.Sum(x => x.ai.CantidadActual) > 0 ? "CON STOCK" : "SIN STOCK"
                                   }).ToListAsync();

            return Ok(new
            {
                FechaReporte = DateTime.UtcNow,
                TotalAmbulancias = resultado.Count,
                AmbulanciasConStock = resultado.Count(r => r.StockTotal > 0),
                Detalle = resultado
            });
        }

        // ================================================
        // ✅ CONSULTA 2: GROUP BY + COUNT
        // ================================================
        [HttpGet("misiones-por-prioridad")]
        public async Task<IActionResult> GetMisionesPorPrioridad()
        {
            var resultado = await _context.Misiones
                .GroupBy(m => m.Prioridad)
                .Select(g => new
                {
                    Prioridad = g.Key,
                    CantidadMisiones = g.Count(),
                    Porcentaje = (double)g.Count() / _context.Misiones.Count() * 100,
                    TiempoPromedioRespuestaMinutos = Math.Round(g.Average(m => m.TiempoRespuestaSegundos ?? 0) / 60.0, 1),
                    TiempoMinimoRespuestaMinutos = Math.Round(g.Min(m => m.TiempoRespuestaSegundos ?? 0) / 60.0, 1),
                    TiempoMaximoRespuestaMinutos = Math.Round(g.Max(m => m.TiempoRespuestaSegundos ?? 0) / 60.0, 1)
                })
                .OrderByDescending(g => g.CantidadMisiones)
                .ToListAsync();

            return Ok(new
            {
                FechaReporte = DateTime.UtcNow,
                TotalMisiones = await _context.Misiones.CountAsync(),
                Detalle = resultado
            });
        }

        // ================================================
        // ✅ CONSULTA 3: GROUP BY + SUM
        // ================================================
        [HttpGet("stock-total-por-ambulancia")]
        public async Task<IActionResult> GetStockTotalPorAmbulancia()
        {
            var resultado = await (from ai in _context.AmbulanciaInsumos
                                   join a in _context.Ambulancias on ai.IdAmbulancia equals a.IdAmbulancia
                                   where ai.Estado == "ACTIVO" && a.Estado == "ACTIVO"
                                   group ai by new { a.Codigo, a.Tipo, a.Estado } into g
                                   select new
                                   {
                                       Ambulancia = g.Key.Codigo,
                                       Tipo = g.Key.Tipo,
                                       StockTotal = g.Sum(x => x.CantidadActual),
                                       PromedioPorInsumo = Math.Round(g.Average(x => x.CantidadActual), 1),
                                       Estado = g.Key.Estado
                                   }).ToListAsync();

            return Ok(new
            {
                FechaReporte = DateTime.UtcNow,
                StockAcumulado = resultado.Sum(r => r.StockTotal),
                Detalle = resultado.OrderByDescending(r => r.StockTotal).ToList()
            });
        }

        // ================================================
        // ✅ CONSULTA 4: Búsqueda filtrada por código
        // ================================================
        [HttpGet("buscar-mision/{numeroSolicitud}")]
        public async Task<IActionResult> GetMisionBySolicitud(string numeroSolicitud)
        {
            var mision = await _context.Misiones
                .Where(m => m.NumeroSolicitud == numeroSolicitud)
                .Select(m => new
                {
                    m.NumeroSolicitud,
                    m.FechaHoraLlamada,
                    m.Prioridad,
                    m.Estado,
                    m.DistanciaKm,
                    TiempoRespuestaMinutos = m.TiempoRespuestaSegundos / 60,
                    TiempoRespuestaSegundos = m.TiempoRespuestaSegundos,
                    m.Observaciones,
                    TieneAlertas = _context.AlertasCriticas.Any(a => a.IdMision == m.IdMision),
                    TotalConstantes = _context.ConstantesVitales.Count(c => c.IdMision == m.IdMision)
                })
                .FirstOrDefaultAsync();

            if (mision == null)
                return NotFound($"No se encontró la misión con número de solicitud: {numeroSolicitud}");

            return Ok(mision);
        }

        // ================================================
        // ✅ CONSULTA 5: NOT EXISTS (Ambulancias sin insumos asignados)
        // ================================================
        [HttpGet("ambulancias-sin-insumos")]
        public async Task<IActionResult> GetAmbulanciasSinInsumos()
        {
            var resultado = await _context.Ambulancias
                .Where(a => !_context.AmbulanciaInsumos.Any(ai => ai.IdAmbulancia == a.IdAmbulancia && ai.Estado == "ACTIVO"))
                .Select(a => new
                {
                    a.Codigo,
                    a.Placa,
                    a.Tipo,
                    a.Estado,
                    a.Kilometraje,
                    Mensaje = "⚠️ Esta ambulancia no tiene insumos asignados. Requiere abastecimiento urgente."
                })
                .ToListAsync();

            return Ok(new
            {
                FechaReporte = DateTime.UtcNow,
                TotalAmbulanciasSinInsumos = resultado.Count,
                TotalAmbulancias = await _context.Ambulancias.CountAsync(a => a.Estado == "ACTIVO"),
                PorcentajeSinStock = resultado.Count > 0 ? (double)resultado.Count / await _context.Ambulancias.CountAsync(a => a.Estado == "ACTIVO") * 100 : 0,
                Detalle = resultado
            });
        }

        // ================================================
        // 📊 REPORTE ADICIONAL: Dashboard de KPIs
        // ================================================
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardKPIs()
        {
            var totalMisiones = await _context.Misiones.CountAsync();
            var misionesFinalizadas = await _context.Misiones.CountAsync(m => m.Estado == "FINALIZADA");
            var misionesEnCurso = await _context.Misiones.CountAsync(m => m.Estado == "EN_CURSO");

            var totalAmbulancias = await _context.Ambulancias.CountAsync(a => a.Estado == "ACTIVO");
            var ambulanciasConStock = await (from ai in _context.AmbulanciaInsumos
                                             where ai.Estado == "ACTIVO"
                                             group ai by ai.IdAmbulancia into g
                                             select g.Key).CountAsync();

            var alertasPendientes = await _context.AlertasCriticas.CountAsync(a => !a.Atendida);
            var notificacionesPendientes = await _context.Notificaciones.CountAsync(n => !n.Enviada);

            var tiempoPromedioRespuesta = await _context.Misiones
                .Where(m => m.TiempoRespuestaSegundos.HasValue)
                .AverageAsync(m => m.TiempoRespuestaSegundos ?? 0);

            return Ok(new
            {
                FechaReporte = DateTime.UtcNow,
                Misiones = new
                {
                    Total = totalMisiones,
                    Finalizadas = misionesFinalizadas,
                    EnCurso = misionesEnCurso,
                    TasaFinalizacion = totalMisiones > 0 ? (double)misionesFinalizadas / totalMisiones * 100 : 0
                },
                Ambulancias = new
                {
                    Total = totalAmbulancias,
                    ConStock = ambulanciasConStock,
                    SinStock = totalAmbulancias - ambulanciasConStock,
                    PorcentajeStock = totalAmbulancias > 0 ? (double)ambulanciasConStock / totalAmbulancias * 100 : 0
                },
                Alertas = new
                {
                    Pendientes = alertasPendientes
                },
                Notificaciones = new
                {
                    Pendientes = notificacionesPendientes
                },
                Calidad = new
                {
                    TiempoPromedioRespuestaMinutos = Math.Round(tiempoPromedioRespuesta / 60.0, 1),
                    TiempoPromedioRespuestaSegundos = tiempoPromedioRespuesta
                }
            });
        }
    }
}