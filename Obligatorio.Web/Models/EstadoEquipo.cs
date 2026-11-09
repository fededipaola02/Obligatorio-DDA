using System.Text.Json.Serialization;

namespace Obligatorio.Web.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EstadoEquipo
{
    Disponible = 1,
    EnUso = 2,
    EnMantenimiento = 3
}

