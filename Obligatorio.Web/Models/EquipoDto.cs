using System.Text.Json.Serialization;

namespace Obligatorio.Web.Models;

public class EquipoDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Nombre { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public TipoEquipo Tipo { get; set; }

    [JsonPropertyName("status")]
    public EstadoEquipo Estado { get; set; }

    [JsonPropertyName("isActive")]
    public bool Activo { get; set; }

    [JsonPropertyName("currentReportId")]
    public int? ReporteActualId { get; set; }

    [JsonPropertyName("currentReportTitle")]
    public string? TituloReporteActual { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime FechaCreacion { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? FechaActualizacion { get; set; }
}

