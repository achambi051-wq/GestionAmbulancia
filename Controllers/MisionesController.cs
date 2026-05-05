using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ambulancia_MIS.Data;
using Ambulancia_MIS.Dominio;
using Ambulancia_MIS.DTOs;

namespace Ambulancia_MIS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MisionesController : ControllerBase
    {
        private readonly AmbulanciaContext _context;

        public MisionesController(AmbulanciaContext context)
        {
            _context = context;
        }

        // ================================================
        // CASO DE USO 1: Orden de misión con prioridad
        // ================================================
        [HttpGet("orden-mision/{numeroSolicitud}")]
        public async Task<IActionResult> GetOrdenMision(string numeroSolicitud)
        {
            var mision = await _context.Misiones
                .Where(m => m.NumeroSolicitud == numeroSolicitud)
                .Select(m => new
                {
                    m.NumeroSolicitud,
                    m.FechaHoraLlamada,
                    m.Prioridad,
                    m.Estado,
                    Protocolo = m.Prioridad == "ROJO" ? "ACTIVAR UCI - PREPARAR EQUIPO DE CHOQUE" :
                               m.Prioridad == "AMARILLO" ? "ACTIVAR PROTOCOLO URGENCIA" : "TRASLADO ESTABLE",
                    TiempoEstimadoLlegadaMinutos = 8
                })
                .FirstOrDefaultAsync();

            if (mision == null) return NotFound();
            return Ok(mision);
        }

        // ================================================
        // CASO DE USO 2: Alertas médicas del paciente
        // ================================================
        [HttpGet("alertas-paciente/{documento}")]
        public async Task<IActionResult> GetAlertasPaciente(string documento)
        {
            // Aquí iría consumo real de Gestión de Pacientes (#1)
            var paciente = new
            {
                Documento = documento,
                Nombres = "JUAN PEREZ",
                Alergias = "PENICILINA",
                EnfermedadesBase = "HIPERTENSION",
                AlertaCritica = "ALERGIA A PENICILINA - NO ADMINISTRAR"
            };

            return Ok(paciente);
        }

        // ================================================
        // CASO DE USO 3: Disponibilidad de camas
        // ================================================
        [HttpGet("camas-disponibles/{tipo}")]
        public async Task<IActionResult> GetCamasDisponibles(string tipo)
        {
            // Aquí iría consumo real de Logística (#31)
            var camasDisponibles = new[]
            {
                new { Codigo = "UCI-01", Tipo = "UCI", Disponible = true },
                new { Codigo = "UCI-02", Tipo = "UCI", Disponible = false },
                new { Codigo = "HOSP-01", Tipo = "HOSPITALIZACION", Disponible = true }
            }.Where(c => c.Tipo == tipo);

            return Ok(new { Tipo = tipo, CamasDisponibles = camasDisponibles });
        }

        // ================================================
        // CASO DE USO 4: Zonas con alerta epidemiológica
        // ================================================
        [HttpGet("zonas-alerta-epidemiologica")]
        public async Task<IActionResult> GetZonasAlertaEpidemiologica()
        {
            // Aquí iría consumo real de Control Epidemiológico (#33)
            var zonas = new[]
            {
                new { Zona = "ZONA SUR", NivelRiesgo = "ALTO", Activa = true },
                new { Zona = "ZONA CENTRO", NivelRiesgo = "MEDIO", Activa = true }
            };

            return Ok(new { AlertasActivas = zonas, MensajeTripulacion = "USAR EQUIPO DE PROTECCION NIVEL 4" });
        }

        // ================================================
        // CASO DE USO 5: Historial de constantes vitales
        // ================================================
        [HttpGet("constantes-vitales/{idMision}")]
        public async Task<IActionResult> GetHistorialConstantesVitales(long idMision)
        {
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
                    // EsCritico se calcula automáticamente, no se asigna
                })
                .ToListAsync();

            return Ok(constantes);
        }

        // ================================================
        // CASO DE USO 6: Protocolos de bioseguridad
        // ================================================
        [HttpGet("protocolo-bioseguridad/{tipoResiduo}")]
        public async Task<IActionResult> GetProtocoloBioseguridad(string tipoResiduo)
        {
            // Aquí iría consumo real de Bioseguridad (#23)
            var protocolo = new
            {
                TipoResiduo = tipoResiduo,
                Procedimiento = "USAR GUANTES, MASCARILLA N95 Y BATA",
                NivelProteccion = 3,
                EquipoNecesario = new[] { "GUANTES", "MASCARILLA N95", "PROTECTOR FACIAL", "BATA" }
            };

            return Ok(protocolo);
        }

        // ================================================
        // CASO DE USO 7: Stock de insumos en ambulancia
        // ================================================
        [HttpGet("stock-insumos/{codigoAmbulancia}")]
        public async Task<IActionResult> GetStockInsumosAmbulancia(string codigoAmbulancia)
        {
            var stock = await (from ai in _context.AmbulanciaInsumos
                               join a in _context.Ambulancias on ai.IdAmbulancia equals a.IdAmbulancia
                               join i in _context.Insumos on ai.IdInsumo equals i.IdInsumo
                               where a.Codigo == codigoAmbulancia && a.Estado == "ACTIVO" && ai.Estado == "ACTIVO"
                               select new
                               {
                                   Insumo = i.Nombre,
                                   CodigoInsumo = i.Codigo,
                                   CantidadActual = ai.CantidadActual,
                                   StockMinimo = ai.CantidadMinima,
                                   EstadoStock = ai.CantidadActual <= ai.CantidadMinima ? "CRITICO - REPONER" : "OK"
                               }).ToListAsync();

            return Ok(new { Ambulancia = codigoAmbulancia, Stock = stock });
        }

        // ================================================
        // CASO DE USO 8: Checklist de mantenimiento
        // ================================================
        [HttpGet("checklist-mantenimiento/{codigoActivo}")]
        public async Task<IActionResult> GetChecklistMantenimiento(string codigoActivo)
        {
            // Aquí iría consumo real de Mantenimiento (#20)
            var checklist = new
            {
                Vehiculo = codigoActivo,
                FechaRevision = DateTime.UtcNow,
                Items = new[]
                {
                    new { Item = "LUCES", Estado = "OK" },
                    new { Item = "FRENOS", Estado = "OK" },
                    new { Item = "NEUMATICOS", Estado = "OK" },
                    new { Item = "SIRENAS", Estado = "OK" }
                },
                EstadoGeneral = "APTO PARA SERVICIO",
                Kilometraje = 15230
            };

            return Ok(checklist);
        }

        // ================================================
        // CASO DE USO 9: Estado de desinfección terminal
        // ================================================
        [HttpGet("estado-desinfeccion/{codigoVehiculo}")]
        public async Task<IActionResult> GetEstadoDesinfeccionTerminal(string codigoVehiculo)
        {
            // Aquí iría consumo real de Bioseguridad (#23)
            var desinfeccion = new
            {
                CodigoVehiculo = codigoVehiculo,
                Certificado = "DES-001-2025",
                FechaDesinfeccion = DateTime.UtcNow.AddDays(-2),
                Vigente = true,
                ProximoControl = DateTime.UtcNow.AddDays(5),
                Estado = "CERTIFICADA"
            };

            return Ok(desinfeccion);
        }

        // ================================================
        // CASO DE USO 10: Indicadores de tiempo respuesta (CORREGIDO)
        // ================================================
        [HttpGet("indicadores-tiempo-respuesta")]
        public async Task<IActionResult> GetIndicadoresTiempoRespuesta()
        {
            // 🔧 CORREGIDO: Obtener datos primero y luego procesar
            var misiones = await _context.Misiones
                .Where(m => m.TiempoRespuestaSegundos.HasValue)
                .Select(m => new { m.Prioridad, m.TiempoRespuestaSegundos })
                .ToListAsync();

            var indicadores = misiones
                .GroupBy(m => m.Prioridad)
                .Select(g => new
                {
                    Prioridad = g.Key,
                    TotalMisiones = g.Count(),
                    TiempoPromedioSegundos = g.Average(m => m.TiempoRespuestaSegundos ?? 0),
                    TiempoMinimoSegundos = g.Min(m => m.TiempoRespuestaSegundos ?? 0),
                    TiempoMaximoSegundos = g.Max(m => m.TiempoRespuestaSegundos ?? 0)
                })
                .Select(g => new
                {
                    g.Prioridad,
                    g.TotalMisiones,
                    TiempoPromedioMinutos = g.TiempoPromedioSegundos / 60,
                    TiempoMaximoPermitidoMinutos = g.Prioridad == "ROJO" ? 8 : g.Prioridad == "AMARILLO" ? 15 : 30,
                    CumpleProtocolo = (g.TiempoPromedioSegundos / 60) <= (g.Prioridad == "ROJO" ? 8 : g.Prioridad == "AMARILLO" ? 15 : 30)
                })
                .ToList();

            var tasaCumplimiento = indicadores.Count > 0
                ? indicadores.Count(i => i.CumpleProtocolo) * 100 / indicadores.Count
                : 0;

            var resumen = new
            {
                KPIs = indicadores,
                TasaCumplimientoGlobal = tasaCumplimiento,
                FechaEvaluacion = DateTime.UtcNow,
                TotalMisionesAnalizadas = misiones.Count
            };

            return Ok(resumen);
        }

        // ================================================
        // ENDPOINTS ADICIONALES
        // ================================================

        // GET: api/Misiones - Listar todas las misiones
        [HttpGet]
        public async Task<IActionResult> GetMisiones()
        {
            var misiones = await _context.Misiones
                .OrderByDescending(m => m.FechaHoraLlamada)
                .Select(m => new MisionDTO
                {
                    NumeroSolicitud = m.NumeroSolicitud,
                    FechaHoraLlamada = m.FechaHoraLlamada,
                    Prioridad = m.Prioridad,
                    Estado = m.Estado,
                    DistanciaKm = m.DistanciaKm,
                    TiempoRespuestaSegundos = m.TiempoRespuestaSegundos,
                    Observaciones = m.Observaciones
                })
                .ToListAsync();

            return Ok(misiones);
        }

        // GET: api/Misiones/{id} - Buscar misión por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMisionById(long id)
        {
            var mision = await _context.Misiones
                .Where(m => m.IdMision == id)
                .Select(m => new MisionDTO
                {
                    NumeroSolicitud = m.NumeroSolicitud,
                    FechaHoraLlamada = m.FechaHoraLlamada,
                    Prioridad = m.Prioridad,
                    Estado = m.Estado,
                    DistanciaKm = m.DistanciaKm,
                    TiempoRespuestaSegundos = m.TiempoRespuestaSegundos,
                    Observaciones = m.Observaciones
                })
                .FirstOrDefaultAsync();

            if (mision == null) return NotFound();
            return Ok(mision);
        }

        // POST: api/Misiones
        [HttpPost]
        public async Task<IActionResult> CreateMision(MisionDTO misionDto)
        {
            var mision = new Mision
            {
                NumeroSolicitud = misionDto.NumeroSolicitud,
                FechaHoraLlamada = DateTime.UtcNow,
                Prioridad = misionDto.Prioridad,
                Estado = "PENDIENTE",
                DistanciaKm = misionDto.DistanciaKm,
                Observaciones = misionDto.Observaciones
            };

            _context.Misiones.Add(mision);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Misión creada", numeroSolicitud = mision.NumeroSolicitud });
        }

        // PUT: api/Misiones/{id}/iniciar - Iniciar misión
        [HttpPut("{id}/iniciar")]
        public async Task<IActionResult> IniciarMision(long id)
        {
            var mision = await _context.Misiones.FindAsync(id);
            if (mision == null) return NotFound();

            mision.FechaHoraSalida = DateTime.UtcNow;
            mision.Estado = "EN_CURSO";
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Misión iniciada", horaSalida = mision.FechaHoraSalida });
        }

        // PUT: api/Misiones/{id}/llegada-paciente - Registrar llegada al paciente
        [HttpPut("{id}/llegada-paciente")]
        public async Task<IActionResult> RegistrarLlegadaPaciente(long id)
        {
            var mision = await _context.Misiones.FindAsync(id);
            if (mision == null) return NotFound();

            mision.FechaLlegadaPaciente = DateTime.UtcNow;

            if (mision.FechaHoraSalida.HasValue)
            {
                mision.TiempoRespuestaSegundos = (int)(mision.FechaLlegadaPaciente.Value - mision.FechaHoraSalida.Value).TotalSeconds;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Llegada al paciente registrada",
                tiempoRespuestaSegundos = mision.TiempoRespuestaSegundos,
                tiempoRespuestaMinutos = mision.TiempoRespuestaSegundos / 60
            });
        }

        // PUT: api/Misiones/{id}/llegada-hospital - Cerrar misión
        [HttpPut("{id}/llegada-hospital")]
        public async Task<IActionResult> RegistrarLlegadaHospital(long id)
        {
            var mision = await _context.Misiones.FindAsync(id);
            if (mision == null) return NotFound();

            mision.FechaLlegadaHospital = DateTime.UtcNow;
            mision.Estado = "FINALIZADA";
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Misión finalizada", horaLlegada = mision.FechaLlegadaHospital });
        }
    }
}