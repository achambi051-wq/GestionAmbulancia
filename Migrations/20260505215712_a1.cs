using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ambulancia_MIS.Migrations
{
    /// <inheritdoc />
    public partial class a1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertasCriticas",
                columns: table => new
                {
                    IdAlerta = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdMision = table.Column<long>(type: "bigint", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Gravedad = table.Column<string>(type: "text", nullable: false),
                    FechaAlerta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Atendida = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertasCriticas", x => x.IdAlerta);
                });

            migrationBuilder.CreateTable(
                name: "Ambulancias",
                columns: table => new
                {
                    IdAmbulancia = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    Placa = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Marca = table.Column<string>(type: "text", nullable: false),
                    Modelo = table.Column<string>(type: "text", nullable: false),
                    AnioFabricacion = table.Column<int>(type: "integer", nullable: false),
                    CapacidadOxigenoLitros = table.Column<int>(type: "integer", nullable: false),
                    TieneDesfibrilador = table.Column<bool>(type: "boolean", nullable: false),
                    Kilometraje = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ambulancias", x => x.IdAmbulancia);
                });

            migrationBuilder.CreateTable(
                name: "ConstantesVitales",
                columns: table => new
                {
                    IdConstante = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdMision = table.Column<long>(type: "bigint", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FrecuenciaCardiaca = table.Column<int>(type: "integer", nullable: false),
                    PresionSistolica = table.Column<int>(type: "integer", nullable: false),
                    PresionDiastolica = table.Column<int>(type: "integer", nullable: false),
                    SaturacionOxigeno = table.Column<int>(type: "integer", nullable: false),
                    FrecuenciaRespiratoria = table.Column<int>(type: "integer", nullable: false),
                    Temperatura = table.Column<decimal>(type: "numeric", nullable: false),
                    Glasgow = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConstantesVitales", x => x.IdConstante);
                });

            migrationBuilder.CreateTable(
                name: "HistorialUbicaciones",
                columns: table => new
                {
                    IdHistorial = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdMision = table.Column<long>(type: "bigint", nullable: false),
                    IdAmbulancia = table.Column<long>(type: "bigint", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Latitud = table.Column<decimal>(type: "numeric", nullable: false),
                    Longitud = table.Column<decimal>(type: "numeric", nullable: false),
                    VelocidadKmh = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialUbicaciones", x => x.IdHistorial);
                });

            migrationBuilder.CreateTable(
                name: "Insumos",
                columns: table => new
                {
                    IdInsumo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Categoria = table.Column<string>(type: "text", nullable: false),
                    UnidadMedida = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    StockMinimo = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric", nullable: false),
                    RequiereRefrigeracion = table.Column<bool>(type: "boolean", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insumos", x => x.IdInsumo);
                });

            migrationBuilder.CreateTable(
                name: "Misiones",
                columns: table => new
                {
                    IdMision = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroSolicitud = table.Column<string>(type: "text", nullable: false),
                    FechaHoraLlamada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaHoraSalida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaLlegadaPaciente = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaLlegadaHospital = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Prioridad = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    LatOrigen = table.Column<decimal>(type: "numeric", nullable: false),
                    LngOrigen = table.Column<decimal>(type: "numeric", nullable: false),
                    LatDestino = table.Column<decimal>(type: "numeric", nullable: false),
                    LngDestino = table.Column<decimal>(type: "numeric", nullable: false),
                    DistanciaKm = table.Column<decimal>(type: "numeric", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: false),
                    TiempoRespuestaSegundos = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Misiones", x => x.IdMision);
                });

            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    IdNotificacion = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdMision = table.Column<long>(type: "bigint", nullable: false),
                    Destinatario = table.Column<string>(type: "text", nullable: false),
                    Mensaje = table.Column<string>(type: "text", nullable: false),
                    Canal = table.Column<string>(type: "text", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Enviada = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.IdNotificacion);
                });

            migrationBuilder.CreateTable(
                name: "AmbulanciaInsumos",
                columns: table => new
                {
                    IdAmbulancia = table.Column<long>(type: "bigint", nullable: false),
                    IdInsumo = table.Column<long>(type: "bigint", nullable: false),
                    CantidadActual = table.Column<int>(type: "integer", nullable: false),
                    CantidadMinima = table.Column<int>(type: "integer", nullable: false),
                    FechaUltimaReposicion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Lote = table.Column<string>(type: "text", nullable: false),
                    FechaCaducidad = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UbicacionAlmacen = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    AmbulanciaIdAmbulancia = table.Column<long>(type: "bigint", nullable: true),
                    InsumoIdInsumo = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmbulanciaInsumos", x => new { x.IdAmbulancia, x.IdInsumo });
                    table.ForeignKey(
                        name: "FK_AmbulanciaInsumos_Ambulancias_AmbulanciaIdAmbulancia",
                        column: x => x.AmbulanciaIdAmbulancia,
                        principalTable: "Ambulancias",
                        principalColumn: "IdAmbulancia");
                    table.ForeignKey(
                        name: "FK_AmbulanciaInsumos_Insumos_InsumoIdInsumo",
                        column: x => x.InsumoIdInsumo,
                        principalTable: "Insumos",
                        principalColumn: "IdInsumo");
                });

            migrationBuilder.CreateTable(
                name: "PlantillaInsumos",
                columns: table => new
                {
                    IdPlantilla = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdInsumo = table.Column<long>(type: "bigint", nullable: false),
                    CantidadRequerida = table.Column<int>(type: "integer", nullable: false),
                    EsObligatorio = table.Column<bool>(type: "boolean", nullable: false),
                    NivelCritico = table.Column<int>(type: "integer", nullable: false),
                    InsumoIdInsumo = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillaInsumos", x => x.IdPlantilla);
                    table.ForeignKey(
                        name: "FK_PlantillaInsumos_Insumos_InsumoIdInsumo",
                        column: x => x.InsumoIdInsumo,
                        principalTable: "Insumos",
                        principalColumn: "IdInsumo");
                });

            migrationBuilder.InsertData(
                table: "AmbulanciaInsumos",
                columns: new[] { "IdAmbulancia", "IdInsumo", "AmbulanciaIdAmbulancia", "CantidadActual", "CantidadMinima", "Estado", "FechaCaducidad", "FechaUltimaReposicion", "InsumoIdInsumo", "Lote", "UbicacionAlmacen" },
                values: new object[,]
                {
                    { 1L, 1L, null, 10, 5, "ACTIVO", null, new DateTime(2026, 5, 5, 21, 57, 4, 92, DateTimeKind.Utc).AddTicks(9161), null, "LOTE-001", "" },
                    { 1L, 2L, null, 2000, 500, "ACTIVO", null, new DateTime(2026, 5, 5, 21, 57, 4, 92, DateTimeKind.Utc).AddTicks(9161), null, "LOTE-002", "" },
                    { 1L, 3L, null, 50, 20, "ACTIVO", null, new DateTime(2026, 5, 5, 21, 57, 4, 92, DateTimeKind.Utc).AddTicks(9161), null, "LOTE-003", "" },
                    { 1L, 4L, null, 2, 2, "ACTIVO", null, new DateTime(2026, 5, 5, 21, 57, 4, 92, DateTimeKind.Utc).AddTicks(9161), null, "LOTE-004", "" },
                    { 2L, 1L, null, 8, 5, "ACTIVO", null, new DateTime(2026, 5, 5, 21, 57, 4, 92, DateTimeKind.Utc).AddTicks(9161), null, "LOTE-005", "" },
                    { 2L, 2L, null, 1500, 500, "ACTIVO", null, new DateTime(2026, 5, 5, 21, 57, 4, 92, DateTimeKind.Utc).AddTicks(9161), null, "LOTE-006", "" },
                    { 2L, 3L, null, 45, 20, "ACTIVO", null, new DateTime(2026, 5, 5, 21, 57, 4, 92, DateTimeKind.Utc).AddTicks(9161), null, "LOTE-007", "" }
                });

            migrationBuilder.InsertData(
                table: "Ambulancias",
                columns: new[] { "IdAmbulancia", "AnioFabricacion", "CapacidadOxigenoLitros", "Codigo", "Estado", "Kilometraje", "Marca", "Modelo", "Placa", "TieneDesfibrilador", "Tipo" },
                values: new object[,]
                {
                    { 1L, 2020, 2000, "AMB-001", "ACTIVO", 15000, "FORD", "TRANSIT", "1234ABC", true, "UCI_MOVIL" },
                    { 2L, 2021, 1000, "AMB-002", "ACTIVO", 8000, "MERCEDES", "SPRINTER", "5678DEF", false, "BASICA" }
                });

            migrationBuilder.InsertData(
                table: "Insumos",
                columns: new[] { "IdInsumo", "Categoria", "Codigo", "Descripcion", "Estado", "Nombre", "PrecioUnitario", "RequiereRefrigeracion", "StockMinimo", "UnidadMedida" },
                values: new object[,]
                {
                    { 1L, "FARMACO", "INS-ADR-01", "", "ACTIVO", "ADRENALINA", 12.50m, false, 5, "AMPOLIA" },
                    { 2L, "GAS", "INS-OXI-01", "", "ACTIVO", "OXIGENO", 5.00m, false, 500, "LITRO" },
                    { 3L, "CURACION", "INS-GAS-01", "", "ACTIVO", "GASA ESTERIL", 0.50m, false, 20, "UNIDAD" },
                    { 4L, "HEMODERIVADO", "INS-SAN-01", "", "ACTIVO", "SANGRE TIPO O+", 150.00m, false, 2, "UNIDAD" }
                });

            migrationBuilder.InsertData(
                table: "Misiones",
                columns: new[] { "IdMision", "DistanciaKm", "Estado", "FechaHoraLlamada", "FechaHoraSalida", "FechaLlegadaHospital", "FechaLlegadaPaciente", "LatDestino", "LatOrigen", "LngDestino", "LngOrigen", "NumeroSolicitud", "Observaciones", "Prioridad", "TiempoRespuestaSegundos" },
                values: new object[] { 1L, 5.2m, "FINALIZADA", new DateTime(2026, 5, 5, 19, 57, 4, 92, DateTimeKind.Utc).AddTicks(9161), null, null, null, -17.3845m, -17.3935m, -66.1468m, -66.1570m, "SOL-2025-001", "PACIENTE CON PARO CARDIACO", "ROJO", 480 });

            migrationBuilder.InsertData(
                table: "PlantillaInsumos",
                columns: new[] { "IdPlantilla", "CantidadRequerida", "EsObligatorio", "IdInsumo", "InsumoIdInsumo", "NivelCritico" },
                values: new object[,]
                {
                    { 1L, 10, true, 1L, null, 3 },
                    { 2L, 2000, true, 2L, null, 500 },
                    { 3L, 50, true, 3L, null, 10 },
                    { 4L, 2, true, 4L, null, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AmbulanciaInsumos_AmbulanciaIdAmbulancia",
                table: "AmbulanciaInsumos",
                column: "AmbulanciaIdAmbulancia");

            migrationBuilder.CreateIndex(
                name: "IX_AmbulanciaInsumos_InsumoIdInsumo",
                table: "AmbulanciaInsumos",
                column: "InsumoIdInsumo");

            migrationBuilder.CreateIndex(
                name: "IX_Misiones_NumeroSolicitud",
                table: "Misiones",
                column: "NumeroSolicitud",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlantillaInsumos_InsumoIdInsumo",
                table: "PlantillaInsumos",
                column: "InsumoIdInsumo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertasCriticas");

            migrationBuilder.DropTable(
                name: "AmbulanciaInsumos");

            migrationBuilder.DropTable(
                name: "ConstantesVitales");

            migrationBuilder.DropTable(
                name: "HistorialUbicaciones");

            migrationBuilder.DropTable(
                name: "Misiones");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "PlantillaInsumos");

            migrationBuilder.DropTable(
                name: "Ambulancias");

            migrationBuilder.DropTable(
                name: "Insumos");
        }
    }
}
