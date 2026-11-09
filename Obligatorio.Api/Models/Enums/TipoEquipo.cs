using System.Text.Json.Serialization;

namespace Obligatorio.Api.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoEquipo
{
    Vehiculo = 1,
    Herramienta = 2,
    Material = 3
}

