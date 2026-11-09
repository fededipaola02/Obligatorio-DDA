using Obligatorio.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Obligatorio.Api.Data.Configurations;

public class AuditoriaEquipoConfiguration : IEntityTypeConfiguration<AuditoriaEquipo>
{
    public void Configure(EntityTypeBuilder<AuditoriaEquipo> builder)
    {
        builder.ToTable("AuditoriasEquipos");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Accion)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.UsuarioId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.FechaHora)
            .IsRequired();

        builder.Property(a => a.Detalles)
            .HasMaxLength(250);

        builder.Property(a => a.EstadoAnterior)
            .HasConversion<int?>();

        builder.Property(a => a.EstadoNuevo)
            .HasConversion<int?>();

        builder.HasIndex(a => a.EquipoId);
        builder.HasIndex(a => a.FechaHora);
    }
}

