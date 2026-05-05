namespace Ambulancia_MIS.DTOs
{
    public class InsumoDTO
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
    }
}