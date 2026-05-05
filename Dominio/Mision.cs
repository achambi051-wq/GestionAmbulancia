using System.ComponentModel.DataAnnotations;

namespace Ambulancia_MIS.Dominio
{
    public class Mision
    {
        [Key]
        public long IdMision { get; set; }
        public string NumeroSolicitud { get; set; } = string.Empty;
        public DateTime FechaHoraLlamada { get; set; }
        public DateTime? FechaHoraSalida { get; set; }
        public DateTime? FechaLlegadaPaciente { get; set; }
        public DateTime? FechaLlegadaHospital { get; set; }
        public string Prioridad { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal LatOrigen { get; set; }
        public decimal LngOrigen { get; set; }
        public decimal LatDestino { get; set; }
        public decimal LngDestino { get; set; }
        public decimal DistanciaKm { get; set; }
        public string Observaciones { get; set; } = string.Empty;
        public int? TiempoRespuestaSegundos { get; set; }
    }
}