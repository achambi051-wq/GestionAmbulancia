namespace Ambulancia_MIS.DTOs
{
    public class HistorialUbicacionDTO
    {
        public DateTime FechaHora { get; set; }
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public int VelocidadKmh { get; set; }
    }
}