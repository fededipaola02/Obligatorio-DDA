using System.ComponentModel.DataAnnotations;
using Obligatorio.Api.Models.Enums;

namespace Obligatorio.Api.Models.Entities;

public class AuditoriaEquipo
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EquipoId { get; set; }

    public virtual Equipo Equipo { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Accion { get; set; } = string.Empty;

    public EstadoEquipo? EstadoAnterior { get; set; }

    public EstadoEquipo? EstadoNuevo { get; set; }

    public int? ReporteId { get; set; }

    [Required]
    [MaxLength(100)]
    public string UsuarioId { get; set; } = string.Empty;

    public DateTime FechaHora { get; set; } = DateTime.UtcNow;

    [MaxLength(250)]
    public string? Detalles { get; set; }
}

