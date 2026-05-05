using System.ComponentModel.DataAnnotations;

namespace Ambulancia_MIS.Dominio
{
    public class Notificacion
    {
        [Key]
        public long IdNotificacion { get; set; }
        public long IdMision { get; set; }
        public string Destinatario { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Canal { get; set; } = string.Empty;
        public DateTime FechaEnvio { get; set; }
        public bool Enviada { get; set; }
    }
}