namespace Ambulancia_MIS.DTOs
{
    public class SituacionActualDTO
    {
        public string CodigoAmbulancia { get; set; } = string.Empty;
        public string TipoAmbulancia { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaConsulta { get; set; }
        public List<InsumoSituacionDTO> Insumos { get; set; } = new();
        public ResumenSituacionDTO Resumen { get; set; } = new();
    }

    public class InsumoSituacionDTO
    {
        public string CodigoInsumo { get; set; } = string.Empty;
        public string NombreInsumo { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public int CantidadRequerida { get; set; }
        public int CantidadActual { get; set; }

 
        public int Diferencia { get; set; }
        public string Estado { get; set; } = string.Empty;
        public bool EsCritico { get; set; }
        public int CantidadAReponer { get; set; }
    }

    public class ResumenSituacionDTO
    {
        public int TotalInsumosRequeridos { get; set; }
        public int TotalInsumosActuales { get; set; }
        public int TotalFaltantes { get; set; }
        public int TotalCriticos { get; set; }

        //  usar { get; set; }
        public bool RequiereReposicionUrgente { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}