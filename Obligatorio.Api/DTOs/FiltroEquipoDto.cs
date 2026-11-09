using Obligatorio.Api.Models.Enums;

namespace Obligatorio.Api.DTOs;

/// <summary>
/// Parámetros opcionales para listar equipos con filtros.
/// </summary>
public class FiltroEquipoDto
{
    public TipoEquipo? Tipo { get; set; }
    public EstadoEquipo? Estado { get; set; }
    public string? Buscar { get; set; }
    public string? OrdenarPor { get; set; }
    public bool SoloActivos { get; set; } = true;
}
