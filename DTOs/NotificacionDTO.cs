namespace Ambulancia_MIS.DTOs
{
    public class NotificacionDTO
    {
        public string Destinatario { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Canal { get; set; } = string.Empty;
        public DateTime FechaEnvio { get; set; }
        public bool Enviada { get; set; }
    }
}