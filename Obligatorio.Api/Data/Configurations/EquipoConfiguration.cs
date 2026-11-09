using Obligatorio.Api.Models.Entities;
using Obligatorio.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Obligatorio.Api.Data.Configurations;

public class EquipoConfiguration : IEntityTypeConfiguration<Equipo>
{
    public void Configure(EntityTypeBuilder<Equipo> builder)
    {
        builder.ToTable("Equipos");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Tipo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Estado)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(EstadoEquipo.Disponible);

        builder.Property(e => e.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.FechaCreacion)
            .IsRequired();

        // Índices para optimizar filtros frecuentes
        builder.HasIndex(e => e.Tipo);
        builder.HasIndex(e => e.Estado);
        builder.HasIndex(e => e.Activo);
        builder.HasIndex(e => e.Nombre);

        // Relación con ReporteActual (1 reporte puede tener N equipos asignados)
        builder.HasOne(e => e.ReporteActual)
            .WithMany(r => r.EquiposAsignados)
            .HasForeignKey(e => e.ReporteActualId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relación con Auditorías (1 equipo tiene N registros de auditoría)
        builder.HasMany(e => e.Auditorias)
            .WithOne(a => a.Equipo)
            .HasForeignKey(a => a.EquipoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

