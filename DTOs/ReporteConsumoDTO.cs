// DTOs/ReporteConsumoDTO.cs
namespace Ambulancia_MIS.DTOs
{
    /// <summary>
    /// Reporte para Facturación/Finanzas
    /// Resume todo lo consumido en una misión
    /// </summary>
    public class ReporteConsumoDTO
    {
        public string NumeroSolicitud { get; set; } = string.Empty;
        public string DocumentoPaciente { get; set; } = string.Empty;
        public DateTime FechaMision { get; set; }
        public string Prioridad { get; set; } = string.Empty;

        // Datos de la ambulancia
        public string CodigoAmbulancia { get; set; } = string.Empty;
        public int KilometrajeRecorrido { get; set; }
        public decimal CostoKilometraje { get; set; }

        // Consumo de insumos
        public List<ConsumoDetalleDTO> InsumosConsumidos { get; set; } = new();
        public decimal TotalInsumos { get; set; }

        // Personal
        public string ParamedicoAsignado { get; set; } = string.Empty;
        public string ConductorAsignado { get; set; } = string.Empty;
        public decimal CostoPersonal { get; set; }

        // Totales
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