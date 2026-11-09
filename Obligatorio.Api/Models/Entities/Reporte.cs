using System.ComponentModel.DataAnnotations;
using Obligatorio.Api.Models.Enums;

namespace Obligatorio.Api.Models.Entities;

public class Reporte
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public EstadoReporte Estado { get; set; } = EstadoReporte.Reportado;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaResolucion { get; set; }

    public virtual ICollection<Equipo> EquiposAsignados { get; set; } = new List<Equipo>();
}

