using System.ComponentModel.DataAnnotations;

namespace Ambulancia_MIS.Dominio
{
    public class ConstanteVital
    {
        [Key]
        public long IdConstante { get; set; }
        public long IdMision { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int FrecuenciaCardiaca { get; set; }
        public int PresionSistolica { get; set; }
        public int PresionDiastolica { get; set; }
        public int SaturacionOxigeno { get; set; }
        public int FrecuenciaRespiratoria { get; set; }
        public decimal Temperatura { get; set; }
        public int? Glasgow { get; set; }
    }
}