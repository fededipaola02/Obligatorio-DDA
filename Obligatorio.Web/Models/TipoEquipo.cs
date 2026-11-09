using System.Text.Json.Serialization;

namespace Obligatorio.Web.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoEquipo
{
    Vehiculo = 1,
    Herramienta = 2,
    Material = 3
}

