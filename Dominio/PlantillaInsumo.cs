// Dominio/PlantillaInsumo.cs
using System.ComponentModel.DataAnnotations;

namespace Ambulancia_MIS.Dominio
{
    /// <summary>
    /// Define lo que CADA ambulancia DEBE tener según regla de negocio
    /// Esto es la "plantilla" que usa Logística para calcular déficits
    /// </summary>
    public class PlantillaInsumo
    {
        [Key]
        public long IdPlantilla { get; set; }

        /// <summary>Referencia al insumo (ej: ADRENALINA)</summary>
        public long IdInsumo { get; set; }

        /// <summary>Cantidad que TODA ambulancia debe tener</summary>
        public int CantidadRequerida { get; set; }

        /// <summary>Es opcional o mandatorio?</summary>
        public bool EsObligatorio { get; set; } = true;

        /// <summary>Nivel crítico (si baja de esto, alerta)</summary>
        public int NivelCritico { get; set; }

        // Navegación
        public Insumo? Insumo { get; set; }
    }
}