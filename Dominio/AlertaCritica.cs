using System.ComponentModel.DataAnnotations;

namespace Ambulancia_MIS.Dominio
{
    public class AlertaCritica
    {
        [Key]  
        public long IdAlerta { get; set; }
        public long IdMision { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Gravedad { get; set; } = string.Empty;
        public DateTime FechaAlerta { get; set; }
        public bool Atendida { get; set; }
    }
}