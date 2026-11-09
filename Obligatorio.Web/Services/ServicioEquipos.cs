using System.Net.Http.Json;
using System.Text.Json;
using Obligatorio.Web.Models;

namespace Obligatorio.Web.Services;

/// <summary>
/// Servicio cliente HTTP que encapsula todas las llamadas a la API de Equipos.
/// Cada método corresponde a un endpoint REST de Obligatorio.Api.
/// </summary>
public class ServicioEquipos
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ServicioEquipos> _logger;

    public ServicioEquipos(HttpClient httpClient, ILogger<ServicioEquipos> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Consulta los equipos aplicando filtros opcionales de tipo, estado, orden y búsqueda.
    /// Consume GET /api/equipos
    /// </summary>
    public async Task<List<EquipoDto>> ObtenerEquiposAsync(FiltroEquipoDto? filtro = null)
    {
        try
        {
            var parametros = new List<string>();

            if (filtro != null)
            {
                if (filtro.Tipo.HasValue)
                    parametros.Add($"tipo={filtro.Tipo.Value}");

                if (filtro.Estado.HasValue)
                    parametros.Add($"estado={filtro.Estado.Value}");

                if (!string.IsNullOrWhiteSpace(filtro.Buscar))
                    parametros.Add($"buscar={Uri.EscapeDataString(filtro.Buscar.Trim())}");

                if (!string.IsNullOrWhiteSpace(filtro.OrdenarPor))
                    parametros.Add($"ordenarPor={Uri.EscapeDataString(filtro.OrdenarPor)}");

                parametros.Add($"soloActivos={filtro.SoloActivos.ToString().ToLower()}");
            }

            var url = "api/equipos";
            if (parametros.Count > 0)
            {
                url += "?" + string.Join("&", parametros);
            }

            var equipos = await _httpClient.GetFromJsonAsync<List<EquipoDto>>(url);
            return equipos ?? new List<EquipoDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de equipos desde la API");
            throw new HttpRequestException($"No se pudo conectar con la API de recursos: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Obtiene un equipo por su identificador.
    /// Consume GET /api/equipos/{id}
    /// </summary>
    public async Task<EquipoDto?> ObtenerPorIdAsync(int id)
    {
        var respuesta = await _httpClient.GetAsync($"api/equipos/{id}");
        if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!respuesta.IsSuccessStatusCode)
        {
            var error = await ParsearMensajeErrorAsync(respuesta);
            throw new HttpRequestException(error);
        }

        return await respuesta.Content.ReadFromJsonAsync<EquipoDto>();
    }

    /// <summary>
    /// Da de alta un nuevo equipo en el sistema.
    /// Consume POST /api/equipos
    /// </summary>
    public async Task<EquipoDto> CrearAsync(CrearEquipoDto dto)
    {
        var respuesta = await _httpClient.PostAsJsonAsync("api/equipos", dto);
        if (!respuesta.IsSuccessStatusCode)
        {
            var error = await ParsearMensajeErrorAsync(respuesta);
            throw new InvalidOperationException(error);
        }

        var creado = await respuesta.Content.ReadFromJsonAsync<EquipoDto>();
        return creado ?? throw new InvalidOperationException("La API no devolvió el recurso creado.");
    }

    /// <summary>
    /// Modifica los datos de un equipo existente.
    /// Consume PUT /api/equipos/{id}
    /// </summary>
    public async Task<EquipoDto> ActualizarAsync(int id, ActualizarEquipoDto dto)
    {
        var respuesta = await _httpClient.PutAsJsonAsync($"api/equipos/{id}", dto);
        if (!respuesta.IsSuccessStatusCode)
        {
            var error = await ParsearMensajeErrorAsync(respuesta);
            throw new InvalidOperationException(error);
        }

        var modificado = await respuesta.Content.ReadFromJsonAsync<EquipoDto>();
        return modificado ?? throw new InvalidOperationException("La API no devolvió el recurso modificado.");
    }

    /// <summary>
    /// Da de baja lógica a un equipo (Activo = false). No es borrado físico.
    /// Consume DELETE /api/equipos/{id}
    /// </summary>
    public async Task DarDeBajaAsync(int id)
    {
        var respuesta = await _httpClient.DeleteAsync($"api/equipos/{id}");
        if (!respuesta.IsSuccessStatusCode)
        {
            var error = await ParsearMensajeErrorAsync(respuesta);
            throw new InvalidOperationException(error);
        }
    }

    /// <summary>
    /// Asigna un equipo Disponible a un reporte de incidente.
    /// Consume POST /api/equipos/{id}/asignar
    /// </summary>
    public async Task<EquipoDto> AsignarAReporteAsync(int id, AsignarEquipoDto dto)
    {
        var respuesta = await _httpClient.PostAsJsonAsync($"api/equipos/{id}/asignar", dto);
        if (!respuesta.IsSuccessStatusCode)
        {
            var error = await ParsearMensajeErrorAsync(respuesta);
            throw new InvalidOperationException(error);
        }

        var asignado = await respuesta.Content.ReadFromJsonAsync<EquipoDto>();
        return asignado ?? throw new InvalidOperationException("La API no devolvió el recurso asignado.");
    }

    /// <summary>
    /// Libera un equipo de su reporte actual volviendo al estado Disponible.
    /// Consume POST /api/equipos/{id}/liberar
    /// </summary>
    public async Task<EquipoDto> LiberarDeReporteAsync(int id)
    {
        var respuesta = await _httpClient.PostAsync($"api/equipos/{id}/liberar", null);
        if (!respuesta.IsSuccessStatusCode)
        {
            var error = await ParsearMensajeErrorAsync(respuesta);
            throw new InvalidOperationException(error);
        }

        var liberado = await respuesta.Content.ReadFromJsonAsync<EquipoDto>();
        return liberado ?? throw new InvalidOperationException("La API no devolvió el recurso liberado.");
    }

    /// <summary>
    /// Obtiene el historial de auditoría de un equipo.
    /// Consume GET /api/equipos/{id}/auditoria
    /// </summary>
    public async Task<List<AuditoriaEquipoDto>> ObtenerAuditoriaAsync(int id)
    {
        var respuesta = await _httpClient.GetAsync($"api/equipos/{id}/auditoria");
        if (!respuesta.IsSuccessStatusCode)
        {
            var error = await ParsearMensajeErrorAsync(respuesta);
            throw new HttpRequestException(error);
        }

        var registros = await respuesta.Content.ReadFromJsonAsync<List<AuditoriaEquipoDto>>();
        return registros ?? new List<AuditoriaEquipoDto>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Métodos privados de apoyo
    // ─────────────────────────────────────────────────────────────────────────

    private static async Task<string> ParsearMensajeErrorAsync(HttpResponseMessage respuesta)
    {
        try
        {
            var contenido = await respuesta.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(contenido))
            {
                return $"Error HTTP {(int)respuesta.StatusCode}: {respuesta.ReasonPhrase}";
            }

            using var documentoJson = JsonDocument.Parse(contenido);
            var raiz = documentoJson.RootElement;

            // ProblemDetails estándar: campo "detail"
            if (raiz.TryGetProperty("detail", out var detalle))
            {
                return detalle.GetString() ?? contenido;
            }

            // ValidationProblemDetails: campo "errors"
            if (raiz.TryGetProperty("errors", out var errores))
            {
                var listaErrores = new List<string>();
                foreach (var propiedad in errores.EnumerateObject())
                {
                    foreach (var item in propiedad.Value.EnumerateArray())
                    {
                        listaErrores.Add(item.GetString() ?? "");
                    }
                }
                if (listaErrores.Count > 0)
                {
                    return string.Join("; ", listaErrores);
                }
            }

            if (raiz.TryGetProperty("title", out var titulo))
            {
                return titulo.GetString() ?? contenido;
            }

            return contenido;
        }
        catch
        {
            return $"Error HTTP {(int)respuesta.StatusCode}: {respuesta.ReasonPhrase}";
        }
    }
}

