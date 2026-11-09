using System.Text.Json.Serialization;

namespace Obligatorio.Web.Models;

public class AuditoriaEquipoDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("equipmentId")]
    public int EquipoId { get; set; }

    [JsonPropertyName("action")]
    public string Accion { get; set; } = string.Empty;

    [JsonPropertyName("previousStatus")]
    public EstadoEquipo? EstadoAnterior { get; set; }

    [JsonPropertyName("newStatus")]
    public EstadoEquipo? EstadoNuevo { get; set; }

    [JsonPropertyName("reportId")]
    public int? ReporteId { get; set; }

    [JsonPropertyName("userId")]
    public string UsuarioId { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime FechaHora { get; set; }

    [JsonPropertyName("details")]
    public string? Detalles { get; set; }
}

