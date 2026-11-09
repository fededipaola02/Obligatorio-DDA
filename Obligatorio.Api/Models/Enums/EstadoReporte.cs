using System.Text.Json.Serialization;

namespace Obligatorio.Api.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EstadoReporte
{
    Reportado = 1,
    Verificado = 2,
    Asignado = 3,
    EnReparacion = 4,
    Solucionado = 5,
    VerificadoResuelto = 6,
    Rechazado = 7,
    Duplicado = 8
}

