namespace Ambulancia_MIS.DTOs
{
    public class MisionDTO
    {
        public string NumeroSolicitud { get; set; } = string.Empty;
        public DateTime FechaHoraLlamada { get; set; }
        public string Prioridad { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal DistanciaKm { get; set; }
        public int? TiempoRespuestaSegundos { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }
}