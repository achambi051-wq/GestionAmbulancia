using System.ComponentModel.DataAnnotations;

namespace Ambulancia_MIS.Dominio
{
    public class HistorialUbicacion
    {
        [Key]
        public long IdHistorial { get; set; }
        public long IdMision { get; set; }
        public long IdAmbulancia { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public int VelocidadKmh { get; set; }
    }
}