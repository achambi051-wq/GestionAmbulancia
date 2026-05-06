using Ambulancia_MIS.DTOs;

namespace Ambulancia_MIS.Services
{
    /// <summary>
    /// Servicio para consumir API de otros microservicios
    /// </summary>
    public interface IConexionExternaService
    {
        // ==================== EMERGENCIAS (#2) ====================
        Task<EmergenciaExternaDTO?> GetOrdenMisionAsync(string codigoEmergencia);
        Task<bool> RegistrarETAAsync(string codigoEmergencia, DateTime eta);
        Task<List<EmergenciaMisionDTO>> GetMisionesActivasAsync();
        Task<List<EmergenciaMisionDTO>> GetMisionesSinETAAsync();
        Task<bool> TestConexionEmergenciasAsync();

        // ==================== GESTIÓN DE PACIENTES (#1) ====================
        Task<PacienteExternoDTO?> GetPacienteAsync(string documento);
        Task<List<PacienteReporteDTO>> GetPacientesConTipoEstadoAsync();
        Task<bool> ActualizarConstantesVitalesAsync(long idMision, object constantes);
        Task<bool> TestConexionPacientesAsync();

        // ==================== GESTIÓN DE CALIDAD (#22) ====================
        Task<List<InformeCalidadDTO>> GetInformesCalidadAsync();
        Task<InformeCalidadDTO?> GetInformeCalidadByCodigoAsync(string codigo);
        Task<double?> GetPromedioCalidadSistemaAsync();
        Task<List<PromedioDepartamentoDTO>> GetPromedioCalidadPorDepartamentoAsync();
        Task<ProtocoloCalidadExternoDTO?> GetProtocoloCalidadAsync(string prioridad);
        Task<bool> RegistrarKPIAsync(KPIMisionDTO kpi);
        Task<bool> TestConexionCalidadAsync();

        // ==================== MANTENIMIENTO Y ACTIVOS (#20) ====================
        Task<ActivoExternoDTO?> GetActivoAsync(string codigo);
        Task<List<FallaExternaDTO>> GetFallasPendientesAsync(string codigoActivo);
        Task<List<MantenimientoExternoDTO>> GetMantenimientosAsync(string codigoActivo);
        Task<List<ActivoListaDTO>> GetListaActivosAsync();

        // ==================== BIOSEGURIDAD (#23) ====================
        Task<ProtocoloBioseguridadExternoDTO?> GetProtocoloBioseguridadAsync(string tipoResiduo);
        Task<List<ItemStockAlertaDTO>> GetItemsStockAlertaAsync();

        // ==================== RECURSOS HUMANOS (#16) ====================
        Task<PersonalExternoDTO?> GetPersonalAsync(string documento);
        Task<CertificacionExternaDTO?> GetCertificacionesAsync(string documento);

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
