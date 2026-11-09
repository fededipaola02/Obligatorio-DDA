using Obligatorio.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Obligatorio.Api.Data.Configurations;

public class ReporteConfiguration : IEntityTypeConfiguration<Reporte>
{
    public void Configure(EntityTypeBuilder<Reporte> builder)
    {
        builder.ToTable("Reportes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Titulo)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(r => r.Descripcion)
            .HasMaxLength(500);

        builder.Property(r => r.Estado)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.FechaCreacion)
            .IsRequired();

        builder.HasIndex(r => r.Estado);
    }
}

