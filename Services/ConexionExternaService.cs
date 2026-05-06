using System.Text.Json;
using Ambulancia_MIS.DTOs;

namespace Ambulancia_MIS.Services
{
    public class ConexionExternaService : IConexionExternaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<ConexionExternaService> _logger;

        private readonly string _pacientesUrl;
        private readonly string _emergenciasUrl;
        private readonly string _rrhhUrl;
        private readonly string _mantenimientoUrl;
        private readonly string _calidadUrl;
        private readonly string _bioseguridadUrl;
        private readonly string _logisticaUrl;
        private readonly string _epidemiologiaUrl;
        private readonly string _facturacionUrl;

        public ConexionExternaService(HttpClient httpClient, IConfiguration config, ILogger<ConexionExternaService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;

            _pacientesUrl = _config["ExternalServices:GestionPacientesUrl"] ?? "https://gestionpacientes.onrender.com/api";
            _emergenciasUrl = _config["ExternalServices:EmergenciasUrl"] ?? "https://hemergencias-production-82c5.up.railway.app/emergencias-upds/api";
            _rrhhUrl = _config["ExternalServices:RecursosHumanosUrl"] ?? "http://10.77.200.xxx:xxxx/api";
            _mantenimientoUrl = _config["ExternalServices:MantenimientoUrl"] ?? "https://backendmya-production-4e82.up.railway.app/api";
            _calidadUrl = _config["ExternalServices:CalidadUrl"] ?? "https://gestioncalidad3.onrender.com/api";
            _bioseguridadUrl = _config["ExternalServices:BioseguridadUrl"] ?? "https://bycs-production.up.railway.app/api";
            _logisticaUrl = _config["ExternalServices:LogisticaUrl"] ?? "http://10.77.200.246:5225/api";
            _epidemiologiaUrl = _config["ExternalServices:EpidemiologiaUrl"] ?? "http://10.77.200.xxx:xxxx/api";
            _facturacionUrl = _config["ExternalServices:FacturacionUrl"] ?? "http://10.77.200.xxx:xxxx/api";
        }

        // ==================== EMERGENCIAS (#2) ====================
        public async Task<EmergenciaExternaDTO?> GetOrdenMisionAsync(string codigoEmergencia)
        {
            try
            {
                var misiones = await GetMisionesActivasAsync();
                var mision = misiones?.FirstOrDefault(m => m.codigo == codigoEmergencia);
                
                if (mision != null)
                {
                    return new EmergenciaExternaDTO
                    {
                        Codigo = mision.codigo,
                        Prioridad = mision.nivelGravedad,
                        Ubicacion = "Por confirmar en atencion",
                        Observaciones = $"Medico: {mision.medico}"
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar emergencia {codigo}", codigoEmergencia);
                return null;
            }
        }

        public async Task<bool> RegistrarETAAsync(string codigoEmergencia, DateTime eta)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_emergenciasUrl}/ambulancia/eta/{codigoEmergencia}", eta);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar ETA para emergencia {codigo}", codigoEmergencia);
                return false;
            }
        }

        public async Task<List<EmergenciaMisionDTO>> GetMisionesActivasAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_emergenciasUrl}/ambulancia/misiones");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<EmergenciaMisionDTO>>() ?? new();
                }
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar misiones activas");
                return new();
            }
        }

        public async Task<List<EmergenciaMisionDTO>> GetMisionesSinETAAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_emergenciasUrl}/ambulancia/sin-eta");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<EmergenciaMisionDTO>>() ?? new();
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar misiones sin ETA");
                return new();
            }
        }

        // ==================== GESTIÓN DE PACIENTES (#1) ====================
        /// <summary>
        /// Obtiene un paciente por CI
        /// Endpoint real: GET /api/pacientes/
        /// Luego filtramos por CI
        /// </summary>
        public async Task<PacienteExternoDTO?> GetPacienteAsync(string documento)
        {
            try
            {
                // Obtener todos los pacientes y filtrar por CI
                var response = await _httpClient.GetAsync($"{_pacientesUrl}/pacientes/");
                if (response.IsSuccessStatusCode)
                {
                    var pacientes = await response.Content.ReadFromJsonAsync<List<PacienteDjangoDTO>>();
                    var paciente = pacientes?.FirstOrDefault(p => p.ci == documento);
                    
                    if (paciente != null)
                    {
                        return new PacienteExternoDTO
                        {
                            Documento = paciente.ci,
                            Nombres = paciente.nombre,
                            Apellidos = paciente.apellido,
                            Alergias = "No registradas en sistema",
                            EnfermedadesBase = "No registradas en sistema",
                            TieneSeguro = false,
                            TipoSeguro = "NINGUNO"
                        };
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar paciente {documento}", documento);
                return null;
            }
        }

        /// <summary>
        /// Obtiene reporte de pacientes con tipo y estado
        /// Endpoint real: GET /api/reporte/
        /// </summary>
        public async Task<List<PacienteReporteDTO>> GetPacientesConTipoEstadoAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_pacientesUrl}/reporte/");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<PacienteReporteDTO>>() ?? new();
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar reporte de pacientes");
                return new();
            }
        }

        public async Task<bool> ActualizarConstantesVitalesAsync(long idMision, object constantes)
        {
            try
            {
                // Este endpoint no existe en el Django de pacientes
                // Podría implementarse después
                _logger.LogWarning("ActualizarConstantesVitalesAsync no implementado en servicio de pacientes");
                return true; // Simular éxito
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar constantes vitales");
                return false;
            }
        }

        // ==================== GESTIÓN DE CALIDAD (#22) ====================
        /// <summary>
        /// Obtiene todos los informes de calidad (calificaciones)
        /// Endpoint real: GET /api/InformeCalidads
        /// </summary>
        public async Task<List<InformeCalidadDTO>> GetInformesCalidadAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_calidadUrl}/InformeCalidads");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<InformeCalidadDTO>>() ?? new();
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar informes de calidad");
                return new();
            }
        }

        /// <summary>
        /// Obtiene un informe de calidad por código
        /// Endpoint real: GET /api/InformeCalidads/{codigo}
        /// </summary>
        public async Task<InformeCalidadDTO?> GetInformeCalidadByCodigoAsync(string codigo)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_calidadUrl}/InformeCalidads/{codigo}");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<InformeCalidadDTO>();
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar informe de calidad {codigo}", codigo);
                return null;
            }
        }

        /// <summary>
        /// Obtiene el promedio general de calificaciones del sistema
        /// Endpoint real: GET /api/InformeCalidad_Departamento/reporte/promedio-sistema
        /// </summary>
        public async Task<double?> GetPromedioCalidadSistemaAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_calidadUrl}/InformeCalidad_Departamento/reporte/promedio-sistema");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<PromedioSistemaDTO>();
                    return result?.PromedioSistema;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar promedio de calidad");
                return null;
            }
        }

        /// <summary>
        /// Obtiene el promedio de calificaciones por departamento
        /// Endpoint real: GET /api/InformeCalidad_Departamento/reporte/promedio/general
        /// </summary>
        public async Task<List<PromedioDepartamentoDTO>> GetPromedioCalidadPorDepartamentoAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_calidadUrl}/InformeCalidad_Departamento/reporte/promedio/general");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<PromedioDepartamentoDTO>>() ?? new();
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar promedio por departamento");
                return new();
            }
        }

        public async Task<ProtocoloCalidadExternoDTO?> GetProtocoloCalidadAsync(string prioridad)
        {
            try
            {
                // Calidad no tiene protocolos por prioridad directamente
                // Podemos mapear desde informes de calidad
                var informes = await GetInformesCalidadAsync();
                var informe = informes.FirstOrDefault(i => i.Codigo == prioridad);
                
                if (informe != null)
                {
                    return new ProtocoloCalidadExternoDTO
                    {
                        Prioridad = prioridad,
                        TiempoMaximoRespuestaSegundos = prioridad == "ROJO" ? 480 : prioridad == "AMARILLO" ? 900 : 1800,
                        Descripcion = informe.Descripcion
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar protocolo de calidad para prioridad {prioridad}", prioridad);
                return null;
            }
        }

        public async Task<bool> RegistrarKPIAsync(KPIMisionDTO kpi)
        {
            try
            {
                // Crear un informe de calidad basado en el KPI
                var nuevoInforme = new
                {
                    calificacion = kpi.ProtocoloCumplido ? 5 : 3,
                    descripcion = $"Mision {kpi.NumeroSolicitud} - Tiempo respuesta: {kpi.TiempoRespuestaSegundos}s",
                    fecha = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    codigo = $"KPI-{kpi.NumeroSolicitud}"
                };
                
                var response = await _httpClient.PostAsJsonAsync($"{_calidadUrl}/InformeCalidads", nuevoInforme);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar KPI");
                return false;
            }
        }

        // ==================== MANTENIMIENTO Y ACTIVOS (#20) ====================
        public async Task<ActivoExternoDTO?> GetActivoAsync(string codigo)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_mantenimientoUrl}/Activos/buscar/{codigo}");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<ActivoExternoDTO>();
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar activo {codigo}", codigo);
                return null;
            }
        }

        public async Task<List<FallaExternaDTO>> GetFallasPendientesAsync(string codigoActivo)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_mantenimientoUrl}/Fallas/porActivo/{codigoActivo}");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<FallaExternaDTO>>() ?? new();
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar fallas de {codigo}", codigoActivo);
                return new();
            }
        }

        public async Task<List<MantenimientoExternoDTO>> GetMantenimientosAsync(string codigoActivo)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_mantenimientoUrl}/Mantenimientos/porActivo/{codigoActivo}");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<MantenimientoExternoDTO>>() ?? new();
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar mantenimientos de {codigo}", codigoActivo);
                return new();
            }
        }

        public async Task<List<ActivoListaDTO>> GetListaActivosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_mantenimientoUrl}/Activos/lista");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<ActivoListaDTO>>() ?? new();
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar lista de activos");
                return new();
            }
        }

        // ==================== BIOSEGURIDAD (#23) ====================
        public async Task<ProtocoloBioseguridadExternoDTO?> GetProtocoloBioseguridadAsync(string tipoResiduo)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_bioseguridadUrl}/Protocolos/GET/{tipoResiduo}");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<ProtocoloBioseguridadExternoDTO>();
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar protocolo de bioseguridad {tipo}", tipoResiduo);
                return null;
            }
        }

        public async Task<List<ItemStockAlertaDTO>> GetItemsStockAlertaAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_bioseguridadUrl}/ItemStocks/GET/Alerta");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<ItemStockAlertaDTO>>() ?? new();
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar items con stock bajo");
                return new();
            }
        }

        // ==================== RECURSOS HUMANOS (#16) ====================
        public async Task<PersonalExternoDTO?> GetPersonalAsync(string documento)
        {
            try
            {
                // Pendiente implementación cuando RRHH esté disponible
                _logger.LogWarning("GetPersonalAsync no implementado - RRHH no disponible");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar personal {documento}", documento);
                return null;
            }
        }

        public async Task<CertificacionExternaDTO?> GetCertificacionesAsync(string documento)
        {
            try
            {
                // Pendiente implementación cuando RRHH esté disponible
                _logger.LogWarning("GetCertificacionesAsync no implementado - RRHH no disponible");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar certificaciones de {documento}", documento);
                return null;
            }
        }

        // ==================== LOGÍSTICA HOSPITALARIA (#31) ====================
        public async Task<List<CamaExternaDTO>> GetCamasDisponiblesAsync(string tipo)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_logisticaUrl}/Camas/disponibles/{tipo}");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<CamaExternaDTO>>() ?? new();
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar camas disponibles tipo {tipo}", tipo);
                return new();
            }
        }

        public async Task<bool> SolicitarReposicionInsumosAsync(string codigoAmbulancia, List<ReposicionItemDTO> items)
        {
            try
            {
                var request = new { CodigoAmbulancia = codigoAmbulancia, Items = items };
                var response = await _httpClient.PostAsJsonAsync($"{_logisticaUrl}/Reposiciones/solicitar", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al solicitar reposición para {ambulancia}", codigoAmbulancia);
                return false;
            }
        }

        public async Task<AlmacenStockExternoDTO?> GetStockAlmacenAsync(string codigoInsumo)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_logisticaUrl}/Almacenes/stock/{codigoInsumo}");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<AlmacenStockExternoDTO>();
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar stock de almacén para {codigo}", codigoInsumo);
                return null;
            }
        }

        // ==================== CONTROL EPIDEMIOLÓGICO (#33) ====================
        public async Task<List<AlertaEpidemiologicaExternaDTO>> GetAlertasActivasAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_epidemiologiaUrl}/Alertas/activas");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<AlertaEpidemiologicaExternaDTO>>() ?? new();
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar alertas epidemiológicas");
                return new();
            }
        }

        // ==================== FACTURACIÓN ====================
        public async Task<bool> EnviarFacturaConsumoAsync(ReporteConsumoDTO reporte)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_facturacionUrl}/Facturas/mision", reporte);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar factura de consumo");
                return false;
            }
        }

        // ==================== MÉTODOS AUXILIARES ====================
        public async Task<bool> TestConexionEmergenciasAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_emergenciasUrl}/ambulancia/misiones");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al probar conexión con Emergencias");
                return false;
            }
        }

        public async Task<bool> TestConexionPacientesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_pacientesUrl}/pacientes/");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al probar conexión con Pacientes");
                return false;
            }
        }

        public async Task<bool> TestConexionCalidadAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_calidadUrl}/InformeCalidads");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al probar conexión con Calidad");
                return false;
            }
        }
    }

    // ==================== DTOs para GESTIÓN DE PACIENTES (Django) ====================
    public class PacienteDjangoDTO
    {
        public int id { get; set; }
        public string codigo_paciente { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string apellido { get; set; } = string.Empty;
        public string ci { get; set; } = string.Empty;
        public string fecha_nacimiento { get; set; } = string.Empty;
        public string sexo { get; set; } = string.Empty;
        public string direccion { get; set; } = string.Empty;
        public string telefono { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
    }

    public class PacienteReporteDTO
    {
        public int paciente__id { get; set; }
        public string paciente__nombre { get; set; } = string.Empty;
        public string paciente__apellido { get; set; } = string.Empty;
        public string tipo__descripcion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
    }

    // ==================== DTOs para GESTIÓN DE CALIDAD ====================
    public class InformeCalidadDTO
    {
        public int id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int Calificacion { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class PromedioSistemaDTO
    {
        public double PromedioSistema { get; set; }
    }

    public class PromedioDepartamentoDTO
    {
        public string Departamento { get; set; } = string.Empty;
        public double Promedio { get; set; }
        public int Total { get; set; }
    }

    // ==================== DTOs ESPECÍFICOS PARA EMERGENCIAS ====================
    public class EmergenciaMisionDTO
    {
        public string codigo { get; set; } = string.Empty;
        public string nivelGravedad { get; set; } = string.Empty;
        public DateTime? fechaSalida { get; set; }
        public DateTime? eta { get; set; }
        public string medico { get; set; } = string.Empty;
    }

    // ==================== DTOs para Mantenimiento ====================
    public class ActivoListaDTO
    {
        public string codigo { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string marca { get; set; } = string.Empty;
        public string modelo { get; set; } = string.Empty;
        public string codigoArea { get; set; } = string.Empty;
    }

    // ==================== DTOs para Bioseguridad ====================
    public class ItemStockAlertaDTO
    {
        public string codigo_item { get; set; } = string.Empty;
        public string codigo_inventario { get; set; } = string.Empty;
        public int cantidad { get; set; }
        public int cantidad_alerta { get; set; }
    }

    // ==================== RESTO DE DTOs ====================
    public class PacienteExternoDTO
    {
        public string Documento { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Alergias { get; set; } = string.Empty;
        public string EnfermedadesBase { get; set; } = string.Empty;
        public bool TieneSeguro { get; set; }
        public string TipoSeguro { get; set; } = string.Empty;
    }

    public class EmergenciaExternaDTO
    {
        public string Codigo { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
    }

    public class PersonalExternoDTO
    {
        public string Documento { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class CertificacionExternaDTO
    {
        public string Documento { get; set; } = string.Empty;
        public List<CertificadoItemDTO> Certificaciones { get; set; } = new();
        public bool TodasVigentes { get; set; }
    }

    public class CertificadoItemDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public bool Vigente { get; set; }
    }

    public class ActivoExternoDTO
    {
        public string codigo { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string marca { get; set; } = string.Empty;
        public string modelo { get; set; } = string.Empty;
        public string tipoActivo { get; set; } = string.Empty;
        public string estadoActivo { get; set; } = string.Empty;
    }

    public class FallaExternaDTO
    {
        public string codigo { get; set; } = string.Empty;
        public string codigoActivo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime fechaReporte { get; set; }
        public string prioridad { get; set; } = string.Empty;
        public string estadoFalla { get; set; } = string.Empty;
    }

    public class MantenimientoExternoDTO
    {
        public string codigo { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string estadoMantenimiento { get; set; } = string.Empty;
        public DateTime fechaInicio { get; set; }
        public DateTime? fechaFin { get; set; }
    }

    public class ProtocoloCalidadExternoDTO
    {
        public string Prioridad { get; set; } = string.Empty;
        public int TiempoMaximoRespuestaSegundos { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class ProtocoloBioseguridadExternoDTO
    {
        public string TipoResiduo { get; set; } = string.Empty;
        public string Procedimiento { get; set; } = string.Empty;
        public int NivelProteccion { get; set; }
    }

    public class CamaExternaDTO
    {
        public string Codigo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public bool Disponible { get; set; }
        public string Ubicacion { get; set; } = string.Empty;
    }

    public class AlmacenStockExternoDTO
    {
        public string CodigoInsumo { get; set; } = string.Empty;
        public string NombreInsumo { get; set; } = string.Empty;
        public int StockDisponible { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    public class AlertaEpidemiologicaExternaDTO
    {
        public string Zona { get; set; } = string.Empty;
        public string NivelRiesgo { get; set; } = string.Empty;
        public bool Activa { get; set; }
        public DateTime FechaAlerta { get; set; }
    }

    public class ReposicionItemDTO
    {
        public string CodigoInsumo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }

    public class KPIMisionDTO
    {
        public string NumeroSolicitud { get; set; } = string.Empty;
        public string CodigoAmbulancia { get; set; } = string.Empty;
        public int TiempoRespuestaSegundos { get; set; }
        public int TiempoTrasladoSegundos { get; set; }
        public bool ProtocoloCumplido { get; set; }
        public DateTime FechaMision { get; set; }
    }

    public class ReporteConsumoDTO
    {
        public string NumeroSolicitud { get; set; } = string.Empty;
        public string DocumentoPaciente { get; set; } = string.Empty;
        public DateTime FechaMision { get; set; }
        public string Prioridad { get; set; } = string.Empty;
        public string CodigoAmbulancia { get; set; } = string.Empty;
        public int KilometrajeRecorrido { get; set; }
        public decimal CostoKilometraje { get; set; }
        public List<ConsumoDetalleDTO> InsumosConsumidos { get; set; } = new();
        public decimal TotalInsumos { get; set; }
        public string ParamedicoAsignado { get; set; } = string.Empty;
        public string ConductorAsignado { get; set; } = string.Empty;
        public decimal CostoPersonal { get; set; }
        public decimal CostoTotalMision { get; set; }
        public string EstadoPago { get; set; } = "PENDIENTE";
    }

    public class ConsumoDetalleDTO
    {
        public string Insumo { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
