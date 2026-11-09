using System.Net.Http.Json;
using System.Text.Json;
using Obligatorio.Web.Models;

namespace Obligatorio.Web.Services;

public class EquipmentApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EquipmentApiService> _logger;

    public EquipmentApiService(HttpClient httpClient, ILogger<EquipmentApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Consulta los equipos aplicando filtros opcionales de tipo, estado, orden y búsqueda.
    /// Consume GET /api/equipment
    /// </summary>
    public async Task<List<EquipoDto>> GetEquipmentsAsync(FiltroEquipoDto? filtro = null)
    {
        try
        {
            var queryParams = new List<string>();

            if (filtro != null)
            {
                if (filtro.Tipo.HasValue)
                    queryParams.Add($"type={filtro.Tipo.Value}");

                if (filtro.Estado.HasValue)
                    queryParams.Add($"status={filtro.Estado.Value}");

                if (!string.IsNullOrWhiteSpace(filtro.Buscar))
                    queryParams.Add($"search={Uri.EscapeDataString(filtro.Buscar.Trim())}");

                if (!string.IsNullOrWhiteSpace(filtro.OrdenarPor))
                    queryParams.Add($"sortBy={Uri.EscapeDataString(filtro.OrdenarPor)}");

                queryParams.Add($"onlyActive={filtro.SoloActivos.ToString().ToLower()}");
            }

            var url = "api/equipment";
            if (queryParams.Count > 0)
            {
                url += "?" + string.Join("&", queryParams);
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
    /// Obtiene un equipo por su ID.
    /// Consume GET /api/equipment/{id}
    /// </summary>
    public async Task<EquipoDto?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/equipment/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await ParseErrorMessageAsync(response);
            throw new HttpRequestException(error);
        }

        return await response.Content.ReadFromJsonAsync<EquipoDto>();
    }

    /// <summary>
    /// Da de alta un nuevo equipo.
    /// Consume POST /api/equipment
    /// </summary>
    public async Task<EquipoDto> CreateAsync(CrearEquipoDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/equipment", dto);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ParseErrorMessageAsync(response);
            throw new InvalidOperationException(error);
        }

        var creado = await response.Content.ReadFromJsonAsync<EquipoDto>();
        return creado ?? throw new InvalidOperationException("La API no devolvió el recurso creado.");
    }

    /// <summary>
    /// Modifica un equipo existente.
    /// Consume PUT /api/equipment/{id}
    /// </summary>
    public async Task<EquipoDto> UpdateAsync(int id, ActualizarEquipoDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/equipment/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ParseErrorMessageAsync(response);
            throw new InvalidOperationException(error);
        }

        var modificado = await response.Content.ReadFromJsonAsync<EquipoDto>();
        return modificado ?? throw new InvalidOperationException("La API no devolvió el recurso modificado.");
    }

    /// <summary>
    /// Da de baja lógica a un equipo.
    /// Consume DELETE /api/equipment/{id}
    /// </summary>
    public async Task DeactivateAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/equipment/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await ParseErrorMessageAsync(response);
            throw new InvalidOperationException(error);
        }
    }

    /// <summary>
    /// Asigna un equipo Disponible a un reporte de incidente.
    /// Consume POST /api/equipment/{id}/assign
    /// </summary>
    public async Task<EquipoDto> AssignToReportAsync(int id, AsignarEquipoDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/equipment/{id}/assign", dto);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ParseErrorMessageAsync(response);
            throw new InvalidOperationException(error);
        }

        var asignado = await response.Content.ReadFromJsonAsync<EquipoDto>();
        return asignado ?? throw new InvalidOperationException("La API no devolvió el recurso asignado.");
    }

    /// <summary>
    /// Libera un equipo volviendo su estado a Disponible.
    /// Consume POST /api/equipment/{id}/release
    /// </summary>
    public async Task<EquipoDto> ReleaseFromReportAsync(int id)
    {
        var response = await _httpClient.PostAsync($"api/equipment/{id}/release", null);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ParseErrorMessageAsync(response);
            throw new InvalidOperationException(error);
        }

        var liberado = await response.Content.ReadFromJsonAsync<EquipoDto>();
        return liberado ?? throw new InvalidOperationException("La API no devolvió el recurso liberado.");
    }

    /// <summary>
    /// Obtiene el historial de auditoría de un equipo.
    /// Consume GET /api/equipment/{id}/audit
    /// </summary>
    public async Task<List<AuditoriaEquipoDto>> GetAuditLogsAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/equipment/{id}/audit");
        if (!response.IsSuccessStatusCode)
        {
            var error = await ParseErrorMessageAsync(response);
            throw new HttpRequestException(error);
        }

        var logs = await response.Content.ReadFromJsonAsync<List<AuditoriaEquipoDto>>();
        return logs ?? new List<AuditoriaEquipoDto>();
    }

    private static async Task<string> ParseErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return $"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
            }

            using var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;

            // ProblemDetails estándar: "detail"
            if (root.TryGetProperty("detail", out var detailElem))
            {
                return detailElem.GetString() ?? content;
            }

            // ValidationProblemDetails: "errors"
            if (root.TryGetProperty("errors", out var errorsElem))
            {
                var errorList = new List<string>();
                foreach (var prop in errorsElem.EnumerateObject())
                {
                    foreach (var item in prop.Value.EnumerateArray())
                    {
                        errorList.Add(item.GetString() ?? "");
                    }
                }
                if (errorList.Count > 0)
                {
                    return string.Join("; ", errorList);
                }
            }

            if (root.TryGetProperty("title", out var titleElem))
            {
                return titleElem.GetString() ?? content;
            }

            return content;
        }
        catch
        {
            return $"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
        }
    }
}

