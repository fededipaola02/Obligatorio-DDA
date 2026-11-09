using System.ComponentModel.DataAnnotations;

namespace Obligatorio.Api.DTOs;

/// <summary>
/// Datos para asignar un Equipo a un reporte/incidente.
/// </summary>
public class AsignarEquipoDto
{
    [Required(ErrorMessage = "El ID del reporte es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del reporte debe ser mayor a 0.")]
    public int ReporteId { get; set; }
}
