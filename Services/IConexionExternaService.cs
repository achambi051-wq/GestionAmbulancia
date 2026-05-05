using Ambulancia_MIS.DTOs;

namespace Ambulancia_MIS.Services
{
    /// <summary>
    /// Servicio para consumir API de otros microservicios en la red privada
    /// </summary>
    public interface IConexionExternaService
    {
        // ==================== GESTIÓN DE PACIENTES (#1) ====================
        Task<PacienteExternoDTO?> GetPacienteAsync(string documento);
        Task<bool> ActualizarConstantesVitalesAsync(long idMision, object constantes);

        // ==================== EMERGENCIAS (#2) ====================
        Task<EmergenciaExternaDTO?> GetOrdenMisionAsync(string codigoEmergencia);

        // ==================== RECURSOS HUMANOS (#16) ====================
        Task<PersonalExternoDTO?> GetPersonalAsync(string documento);
        Task<CertificacionExternaDTO?> GetCertificacionesAsync(string documento);

        // ==================== MANTENIMIENTO Y ACTIVOS (#20) ====================
        Task<ActivoExternoDTO?> GetActivoAsync(string codigo);
        Task<List<FallaExternaDTO>> GetFallasPendientesAsync(string codigoActivo);
        Task<List<MantenimientoExternoDTO>> GetMantenimientosAsync(string codigoActivo);

        // ==================== GESTIÓN DE CALIDAD (#22) ====================
        Task<ProtocoloCalidadExternoDTO?> GetProtocoloCalidadAsync(string prioridad);
        Task<bool> RegistrarKPIAsync(KPIMisionDTO kpi);

        // ==================== BIOSEGURIDAD (#23) ====================
        Task<ProtocoloBioseguridadExternoDTO?> GetProtocoloBioseguridadAsync(string tipoResiduo);

        // ==================== LOGÍSTICA HOSPITALARIA (#31) ====================
        Task<List<CamaExternaDTO>> GetCamasDisponiblesAsync(string tipo);
        Task<bool> SolicitarReposicionInsumosAsync(string codigoAmbulancia, List<ReposicionItemDTO> items);
        Task<AlmacenStockExternoDTO?> GetStockAlmacenAsync(string codigoInsumo);

        // ==================== CONTROL EPIDEMIOLÓGICO (#33) ====================
        Task<List<AlertaEpidemiologicaExternaDTO>> GetAlertasActivasAsync();

        // ==================== FACTURACIÓN ====================
        Task<bool> EnviarFacturaConsumoAsync(ReporteConsumoDTO reporte);
    }
}