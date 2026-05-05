using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Ambulancia_MIS.Dominio
{
    public class Insumo
    {
        [Key]
        public long IdInsumo { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int StockMinimo { get; set; }
        public decimal PrecioUnitario { get; set; }
        public bool RequiereRefrigeracion { get; set; }
        public string Estado { get; set; } = "ACTIVO";

        [JsonIgnore]
        public List<AmbulanciaInsumo> AmbulanciaInsumos { get; set; } = new();
    }
}