using Microsoft.EntityFrameworkCore;
using Ambulancia_MIS.Dominio;

namespace Ambulancia_MIS.Data
{
    public class AmbulanciaContext : DbContext
    {
        public AmbulanciaContext(DbContextOptions<AmbulanciaContext> options) : base(options) { }

        public DbSet<Ambulancia> Ambulancias { get; set; }
        public DbSet<Insumo> Insumos { get; set; }
        public DbSet<AmbulanciaInsumo> AmbulanciaInsumos { get; set; }
        public DbSet<Mision> Misiones { get; set; }
        public DbSet<ConstanteVital> ConstantesVitales { get; set; }
        public DbSet<AlertaCritica> AlertasCriticas { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<HistorialUbicacion> HistorialUbicaciones { get; set; }
        public DbSet<PlantillaInsumo> PlantillaInsumos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 5FN - Llave compuesta
            modelBuilder.Entity<AmbulanciaInsumo>()
                .HasKey(ai => new { ai.IdAmbulancia, ai.IdInsumo });

            modelBuilder.Entity<Mision>()
                .HasIndex(m => m.NumeroSolicitud)
                .IsUnique();

            // ==================== DATOS SEMILLA ====================
            var utcNow = DateTime.UtcNow;

            // 1. Ambulancias
            modelBuilder.Entity<Ambulancia>().HasData(
                new Ambulancia { IdAmbulancia = 1, Codigo = "AMB-001", Placa = "1234ABC", Tipo = "UCI_MOVIL", Marca = "FORD", Modelo = "TRANSIT", AnioFabricacion = 2020, CapacidadOxigenoLitros = 2000, TieneDesfibrilador = true, Kilometraje = 15000, Estado = "ACTIVO" },
                new Ambulancia { IdAmbulancia = 2, Codigo = "AMB-002", Placa = "5678DEF", Tipo = "BASICA", Marca = "MERCEDES", Modelo = "SPRINTER", AnioFabricacion = 2021, CapacidadOxigenoLitros = 1000, TieneDesfibrilador = false, Kilometraje = 8000, Estado = "ACTIVO" }
            );

            // 2. Insumos
            modelBuilder.Entity<Insumo>().HasData(
                new Insumo { IdInsumo = 1, Codigo = "INS-ADR-01", Nombre = "ADRENALINA", Categoria = "FARMACO", UnidadMedida = "AMPOLIA", StockMinimo = 5, PrecioUnitario = 12.50m, Estado = "ACTIVO" },
                new Insumo { IdInsumo = 2, Codigo = "INS-OXI-01", Nombre = "OXIGENO", Categoria = "GAS", UnidadMedida = "LITRO", StockMinimo = 500, PrecioUnitario = 5.00m, Estado = "ACTIVO" },
                new Insumo { IdInsumo = 3, Codigo = "INS-GAS-01", Nombre = "GASA ESTERIL", Categoria = "CURACION", UnidadMedida = "UNIDAD", StockMinimo = 20, PrecioUnitario = 0.50m, Estado = "ACTIVO" },
                new Insumo { IdInsumo = 4, Codigo = "INS-SAN-01", Nombre = "SANGRE TIPO O+", Categoria = "HEMODERIVADO", UnidadMedida = "UNIDAD", StockMinimo = 2, PrecioUnitario = 150.00m, Estado = "ACTIVO" }
            );

            // 3. Plantilla (lo que CADA ambulancia DEBE tener)
            modelBuilder.Entity<PlantillaInsumo>().HasData(
                new PlantillaInsumo { IdPlantilla = 1, IdInsumo = 1, CantidadRequerida = 10, EsObligatorio = true, NivelCritico = 3 },
                new PlantillaInsumo { IdPlantilla = 2, IdInsumo = 2, CantidadRequerida = 2000, EsObligatorio = true, NivelCritico = 500 },
                new PlantillaInsumo { IdPlantilla = 3, IdInsumo = 3, CantidadRequerida = 50, EsObligatorio = true, NivelCritico = 10 },
                new PlantillaInsumo { IdPlantilla = 4, IdInsumo = 4, CantidadRequerida = 2, EsObligatorio = true, NivelCritico = 1 }
            );

            // 4. Stock inicial de cada ambulancia
            modelBuilder.Entity<AmbulanciaInsumo>().HasData(
                new AmbulanciaInsumo { IdAmbulancia = 1, IdInsumo = 1, CantidadActual = 10, CantidadMinima = 5, FechaUltimaReposicion = utcNow, Lote = "LOTE-001", Estado = "ACTIVO" },
                new AmbulanciaInsumo { IdAmbulancia = 1, IdInsumo = 2, CantidadActual = 2000, CantidadMinima = 500, FechaUltimaReposicion = utcNow, Lote = "LOTE-002", Estado = "ACTIVO" },
                new AmbulanciaInsumo { IdAmbulancia = 1, IdInsumo = 3, CantidadActual = 50, CantidadMinima = 20, FechaUltimaReposicion = utcNow, Lote = "LOTE-003", Estado = "ACTIVO" },
                new AmbulanciaInsumo { IdAmbulancia = 1, IdInsumo = 4, CantidadActual = 2, CantidadMinima = 2, FechaUltimaReposicion = utcNow, Lote = "LOTE-004", Estado = "ACTIVO" },
                new AmbulanciaInsumo { IdAmbulancia = 2, IdInsumo = 1, CantidadActual = 8, CantidadMinima = 5, FechaUltimaReposicion = utcNow, Lote = "LOTE-005", Estado = "ACTIVO" },
                new AmbulanciaInsumo { IdAmbulancia = 2, IdInsumo = 2, CantidadActual = 1500, CantidadMinima = 500, FechaUltimaReposicion = utcNow, Lote = "LOTE-006", Estado = "ACTIVO" },
                new AmbulanciaInsumo { IdAmbulancia = 2, IdInsumo = 3, CantidadActual = 45, CantidadMinima = 20, FechaUltimaReposicion = utcNow, Lote = "LOTE-007", Estado = "ACTIVO" }
            );

            // 5. Misiones de ejemplo
            modelBuilder.Entity<Mision>().HasData(
                new Mision { IdMision = 1, NumeroSolicitud = "SOL-2025-001", FechaHoraLlamada = utcNow.AddHours(-2), Prioridad = "ROJO", Estado = "FINALIZADA", LatOrigen = -17.3935m, LngOrigen = -66.1570m, LatDestino = -17.3845m, LngDestino = -66.1468m, DistanciaKm = 5.2m, Observaciones = "PACIENTE CON PARO CARDIACO", TiempoRespuestaSegundos = 480 }
            );
        }
    }
}