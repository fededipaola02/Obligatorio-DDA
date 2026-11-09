using Obligatorio.Api.Data.Configurations;
using Obligatorio.Api.Models.Entities;
using Obligatorio.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Obligatorio.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Equipo> Equipos => Set<Equipo>();
    public DbSet<AuditoriaEquipo> AuditoriasEquipos => Set<AuditoriaEquipo>();
    public DbSet<Reporte> Reportes => Set<Reporte>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas las configuraciones IEntityTypeConfiguration del ensamblado
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Seed Data para desarrollo y pruebas de reglas de negocio
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var fechaFija = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        // 1. Reportes iniciales
        modelBuilder.Entity<Reporte>().HasData(
            new Reporte
            {
                Id = 1,
                Titulo = "Reparación de luminaria en Av. 18 de Julio",
                Descripcion = "Columna de alumbrado sin funcionamiento cerca del cruce principal.",
                Estado = EstadoReporte.EnReparacion,
                FechaCreacion = fechaFija
            },
            new Reporte
            {
                Id = 2,
                Titulo = "Bache profundo en calle Colonia",
                Descripcion = "Bache peligroso que dificulta el tránsito vehicular.",
                Estado = EstadoReporte.Asignado,
                FechaCreacion = fechaFija.AddDays(1)
            },
            new Reporte
            {
                Id = 3,
                Titulo = "Limpieza de plaza pública y poda",
                Descripcion = "Ramas caídas y acumulación de residuos tras tormenta.",
                Estado = EstadoReporte.Solucionado,
                FechaCreacion = fechaFija.AddDays(-2),
                FechaResolucion = fechaFija.AddDays(-1)
            }
        );

        // 2. Equipos iniciales (diversos tipos y estados)
        modelBuilder.Entity<Equipo>().HasData(
            new Equipo
            {
                Id = 1,
                Nombre = "Camioneta Utilitaria 01",
                Tipo = TipoEquipo.Vehiculo,
                Estado = EstadoEquipo.EnUso,
                Activo = true,
                ReporteActualId = 1,
                FechaCreacion = fechaFija
            },
            new Equipo
            {
                Id = 2,
                Nombre = "Camión Grúa Hidráulica 02",
                Tipo = TipoEquipo.Vehiculo,
                Estado = EstadoEquipo.Disponible,
                Activo = true,
                ReporteActualId = null,
                FechaCreacion = fechaFija
            },
            new Equipo
            {
                Id = 3,
                Nombre = "Martillo Neumático Industrial",
                Tipo = TipoEquipo.Herramienta,
                Estado = EstadoEquipo.EnMantenimiento,
                Activo = true,
                ReporteActualId = null,
                FechaCreacion = fechaFija
            },
            new Equipo
            {
                Id = 4,
                Nombre = "Escalera Telescópica 8m",
                Tipo = TipoEquipo.Herramienta,
                Estado = EstadoEquipo.EnUso,
                Activo = true,
                ReporteActualId = 1,
                FechaCreacion = fechaFija
            },
            new Equipo
            {
                Id = 5,
                Nombre = "Generador Eléctrico 5kW",
                Tipo = TipoEquipo.Herramienta,
                Estado = EstadoEquipo.Disponible,
                Activo = true,
                ReporteActualId = null,
                FechaCreacion = fechaFija
            },
            new Equipo
            {
                Id = 6,
                Nombre = "Conos de Seguridad Vial (x20)",
                Tipo = TipoEquipo.Material,
                Estado = EstadoEquipo.Disponible,
                Activo = true,
                ReporteActualId = null,
                FechaCreacion = fechaFija
            },
            new Equipo
            {
                Id = 7,
                Nombre = "Vallas de Contención Peatonal (x10)",
                Tipo = TipoEquipo.Material,
                Estado = EstadoEquipo.EnUso,
                Activo = true,
                ReporteActualId = 2,
                FechaCreacion = fechaFija
            }
        );

        // 3. Auditoría inicial
        modelBuilder.Entity<AuditoriaEquipo>().HasData(
            new AuditoriaEquipo
            {
                Id = 1,
                EquipoId = 1,
                Accion = "Asignado",
                EstadoAnterior = EstadoEquipo.Disponible,
                EstadoNuevo = EstadoEquipo.EnUso,
                ReporteId = 1,
                UsuarioId = "admin@ciudad.gob",
                FechaHora = fechaFija,
                Detalles = "Asignación inicial para reparación de luminaria"
            },
            new AuditoriaEquipo
            {
                Id = 2,
                EquipoId = 3,
                Accion = "CambioEstado",
                EstadoAnterior = EstadoEquipo.Disponible,
                EstadoNuevo = EstadoEquipo.EnMantenimiento,
                ReporteId = null,
                UsuarioId = "admin@ciudad.gob",
                FechaHora = fechaFija,
                Detalles = "Mantenimiento preventivo programado"
            }
        );
    }
}

