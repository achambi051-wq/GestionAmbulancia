using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Ambulancia_MIS.Dominio
{
    public class Ambulancia
    {
        [Key]
        public long IdAmbulancia { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int AnioFabricacion { get; set; }
        public int CapacidadOxigenoLitros { get; set; }
        public bool TieneDesfibrilador { get; set; }
        public int Kilometraje { get; set; }
        public string Estado { get; set; } = "ACTIVO";

        [JsonIgnore]
        public List<AmbulanciaInsumo> AmbulanciaInsumos { get; set; } = new();
    }
}