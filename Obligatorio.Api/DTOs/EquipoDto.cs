using System;
using Obligatorio.Api.Models.Enums;

namespace Obligatorio.Api.DTOs;

/// <summary>
/// Representa los datos expuestos del recurso Equipo.
/// </summary>
public class EquipoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = default!;
    public TipoEquipo Tipo { get; set; }
    public EstadoEquipo Estado { get; set; }
    public bool Activo { get; set; }
    public int? ReporteActualId { get; set; }
    public string? TituloReporteActual { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}
