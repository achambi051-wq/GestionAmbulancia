namespace Ambulancia_MIS.DTOs
{
    public class AmbulanciaInsumoDTO
    {
        public string CodigoAmbulancia { get; set; } = string.Empty;
        public string CodigoInsumo { get; set; } = string.Empty;
        public string NombreInsumo { get; set; } = string.Empty;
        public int CantidadActual { get; set; }
        public int CantidadMinima { get; set; }
        public string EstadoStock => CantidadActual <= CantidadMinima ? "BAJO" : "OK";
        public string Lote { get; set; } = string.Empty;
        public DateTime? FechaCaducidad { get; set; }
    }
}