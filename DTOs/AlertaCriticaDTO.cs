namespace Ambulancia_MIS.DTOs
{
    public class AlertaCriticaDTO
    {
        public string Tipo { get; set; } = string.Empty;
        public string Gravedad { get; set; } = string.Empty;
        public DateTime FechaAlerta { get; set; }
        public bool Atendida { get; set; }

        //  usar { get; set; } para que sea asignable
        public string Estado { get; set; } = string.Empty;
        public string TiempoTranscurrido { get; set; } = string.Empty;
    }
}