using System.ComponentModel.DataAnnotations;
using Obligatorio.Api.Models.Enums;

namespace Obligatorio.Api.Models.Entities;

public class Equipo
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public TipoEquipo Tipo { get; set; }

    [Required]
    public EstadoEquipo Estado { get; set; } = EstadoEquipo.Disponible;

    public bool Activo { get; set; } = true;

    public int? ReporteActualId { get; set; }

    public virtual Reporte? ReporteActual { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public virtual ICollection<AuditoriaEquipo> Auditorias { get; set; } = new List<AuditoriaEquipo>();
}

