namespace Ambulancia_MIS.DTOs
{
    public class ConstanteVitalDTO
    {
        public DateTime FechaRegistro { get; set; }
        public int FrecuenciaCardiaca { get; set; }
        public int PresionArterialSistolica { get; set; }
        public int PresionArterialDiastolica { get; set; }
        public int SaturacionOxigeno { get; set; }
        public decimal Temperatura { get; set; }

        // usar { get; set; } para que sea asignable
        public bool EsCritico { get; set; }

        public string PresionArterial => $"{PresionArterialSistolica}/{PresionArterialDiastolica}";
    }
}