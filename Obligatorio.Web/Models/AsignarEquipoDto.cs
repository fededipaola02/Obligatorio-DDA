using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Obligatorio.Web.Models;

public class AsignarEquipoDto
{
    [Required(ErrorMessage = "Debe indicar el número de reporte.")]
    [Range(1, int.MaxValue, ErrorMessage = "El Id del reporte debe ser mayor a 0.")]
    [JsonPropertyName("reportId")]
    public int ReporteId { get; set; }
}

