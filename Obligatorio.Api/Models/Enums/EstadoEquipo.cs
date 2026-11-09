using System.Text.Json.Serialization;

namespace Obligatorio.Api.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EstadoEquipo
{
    Disponible = 1,
    EnUso = 2,
    EnMantenimiento = 3
}

