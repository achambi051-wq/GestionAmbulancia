using System.Text.Json.Serialization;

namespace Ambulancia_MIS.Dominio
{
    public class AmbulanciaInsumo
    {
        public long IdAmbulancia { get; set; }
        public long IdInsumo { get; set; }
        public int CantidadActual { get; set; }
        public int CantidadMinima { get; set; }
        public DateTime? FechaUltimaReposicion { get; set; }
        public string Lote { get; set; } = string.Empty;
        public DateTime? FechaCaducidad { get; set; }
        public string UbicacionAlmacen { get; set; } = string.Empty;
        public string Estado { get; set; } = "ACTIVO";

        [JsonIgnore]
        public Ambulancia? Ambulancia { get; set; }

        [JsonIgnore]
        public Insumo? Insumo { get; set; }
    }
}
