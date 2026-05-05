// DTOs/ValidacionPreMisionDTO.cs
namespace Ambulancia_MIS.DTOs
{
    /// <summary>
    /// Resultado de validar si una ambulancia puede salir a misión
    /// Se valida con RRHH y Mantenimiento antes de autorizar
    /// </summary>
    public class ValidacionPreMisionDTO
    {
        public bool Autorizado { get; set; }
        public string CodigoAmbulancia { get; set; } = string.Empty;
        public string CodigoParamedico { get; set; } = string.Empty;
        public string CodigoConductor { get; set; } = string.Empty;
        public List<ValidacionItemDTO> Validaciones { get; set; } = new();
        public string MensajeGeneral { get; set; } = string.Empty;
        public DateTime FechaValidacion { get; set; }
    }

    public class ValidacionItemDTO
    {
        public string Aspecto { get; set; } = string.Empty; // "PERSONAL", "VEHICULO", "INSUMOS"
        public bool Aprobado { get; set; }
        public string Detalle { get; set; } = string.Empty;
        public string Fuente { get; set; } = string.Empty; // "RRHH", "MANTENIMIENTO", "PROPIO"
    }
}