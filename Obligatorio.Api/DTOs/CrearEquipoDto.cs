using System.ComponentModel.DataAnnotations;
using Obligatorio.Api.Models.Enums;

namespace Obligatorio.Api.DTOs;

/// <summary>
/// Campos requeridos para crear un nuevo Equipo.
/// </summary>
public class CrearEquipoDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres.")]
    public string Nombre { get; set; } = default!;

    [Required(ErrorMessage = "El tipo es obligatorio.")]
    public TipoEquipo Tipo { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    public EstadoEquipo Estado { get; set; }
}
