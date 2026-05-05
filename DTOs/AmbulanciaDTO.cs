namespace Ambulancia_MIS.DTOs
{
    public class AmbulanciaDTO
    {
        public string Codigo { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int AnioFabricacion { get; set; }
        public int CapacidadOxigenoLitros { get; set; }
        public bool TieneDesfibrilador { get; set; }
        public int Kilometraje { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}