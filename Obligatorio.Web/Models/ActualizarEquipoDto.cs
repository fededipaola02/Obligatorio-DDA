using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Obligatorio.Web.Models;

public class ActualizarEquipoDto
{
    [Required(ErrorMessage = "El nombre del recurso es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [JsonPropertyName("name")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar un tipo de recurso.")]
    [JsonPropertyName("type")]
    public TipoEquipo Tipo { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un estado.")]
    [JsonPropertyName("status")]
    public EstadoEquipo Estado { get; set; }
}

