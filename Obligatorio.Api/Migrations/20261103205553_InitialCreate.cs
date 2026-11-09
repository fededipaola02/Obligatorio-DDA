using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Obligatorio.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reportes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reportes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Equipos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ReporteActualId = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipos_Reportes_ReporteActualId",
                        column: x => x.ReporteActualId,
                        principalTable: "Reportes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AuditoriasEquipos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipoId = table.Column<int>(type: "int", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EstadoAnterior = table.Column<int>(type: "int", nullable: true),
                    EstadoNuevo = table.Column<int>(type: "int", nullable: true),
                    ReporteId = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Detalles = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriasEquipos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditoriasEquipos_Equipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Equipos",
                columns: new[] { "Id", "Activo", "Estado", "FechaActualizacion", "FechaCreacion", "Nombre", "ReporteActualId", "Tipo" },
                values: new object[,]
                {
                    { 2, true, 1, null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Camión Grúa Hidráulica 02", null, 1 },
                    { 3, true, 3, null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Martillo Neumático Industrial", null, 2 },
                    { 5, true, 1, null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Generador Eléctrico 5kW", null, 2 },
                    { 6, true, 1, null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Conos de Seguridad Vial (x20)", null, 3 }
                });

            migrationBuilder.InsertData(
                table: "Reportes",
                columns: new[] { "Id", "Descripcion", "Estado", "FechaCreacion", "FechaResolucion", "Titulo" },
                values: new object[,]
                {
                    { 1, "Columna de alumbrado sin funcionamiento cerca del cruce principal.", 4, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), null, "Reparación de luminaria en Av. 18 de Julio" },
                    { 2, "Bache peligroso que dificulta el tránsito vehicular.", 3, new DateTime(2026, 1, 16, 10, 0, 0, 0, DateTimeKind.Utc), null, "Bache profundo en calle Colonia" },
                    { 3, "Ramas caídas y acumulación de residuos tras tormenta.", 5, new DateTime(2026, 1, 13, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 14, 10, 0, 0, 0, DateTimeKind.Utc), "Limpieza de plaza pública y poda" }
                });

            migrationBuilder.InsertData(
                table: "AuditoriasEquipos",
                columns: new[] { "Id", "Accion", "Detalles", "EquipoId", "EstadoAnterior", "EstadoNuevo", "FechaHora", "ReporteId", "UsuarioId" },
                values: new object[] { 2, "CambioEstado", "Mantenimiento preventivo programado", 3, 1, 3, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), null, "admin@ciudad.gob" });

            migrationBuilder.InsertData(
                table: "Equipos",
                columns: new[] { "Id", "Activo", "Estado", "FechaActualizacion", "FechaCreacion", "Nombre", "ReporteActualId", "Tipo" },
                values: new object[,]
                {
                    { 1, true, 2, null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Camioneta Utilitaria 01", 1, 1 },
                    { 4, true, 2, null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Escalera Telescópica 8m", 1, 2 },
                    { 7, true, 2, null, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Vallas de Contención Peatonal (x10)", 2, 3 }
                });

            migrationBuilder.InsertData(
                table: "AuditoriasEquipos",
                columns: new[] { "Id", "Accion", "Detalles", "EquipoId", "EstadoAnterior", "EstadoNuevo", "FechaHora", "ReporteId", "UsuarioId" },
                values: new object[] { 1, "Asignado", "Asignación inicial para reparación de luminaria", 1, 1, 2, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 1, "admin@ciudad.gob" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriasEquipos_EquipoId",
                table: "AuditoriasEquipos",
                column: "EquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriasEquipos_FechaHora",
                table: "AuditoriasEquipos",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_Equipos_Activo",
                table: "Equipos",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_Equipos_Estado",
                table: "Equipos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Equipos_Nombre",
                table: "Equipos",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Equipos_ReporteActualId",
                table: "Equipos",
                column: "ReporteActualId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipos_Tipo",
                table: "Equipos",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_Reportes_Estado",
                table: "Reportes",
                column: "Estado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriasEquipos");

            migrationBuilder.DropTable(
                name: "Equipos");

            migrationBuilder.DropTable(
                name: "Reportes");
        }
    }
}
