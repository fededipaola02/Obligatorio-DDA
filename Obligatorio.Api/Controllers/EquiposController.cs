using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Obligatorio.Api.DTOs;
using Obligatorio.Api.Interfaces;
using Obligatorio.Api.Exceptions;

namespace Obligatorio.Api.Controllers;

/// <summary>
/// Endpoints CRUD para el recurso Equipo. Todas las rutas están en español.
/// </summary>
[ApiController]
[Route("api/equipos")]
[Produces("application/json")]
public class EquiposController : ControllerBase
{
    private readonly IEquipoService _servicio;
    private readonly ILogger<EquiposController> _log;

    public EquiposController(IEquipoService servicio, ILogger<EquiposController> log)
    {
        _servicio = servicio;
        _log = log;
    }

    // ── LISTADO ───────────────────────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EquipoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EquipoDto>>> Listar([FromQuery] FiltroEquipoDto filtro) =>
        Ok(await _servicio.ObtenerTodosAsync(filtro));

    // ── DETALLE ───────────────────────────────────────────────────────────────
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EquipoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipoDto>> Obtener(int id)
    {
        try
        {
            return Ok(await _servicio.ObtenerPorIdAsync(id));
        }
        catch (NotFoundException ex)
        {
            return NotFound(CrearProblemDetails(404, "Recurso no encontrado", ex.Message));
        }
    }

    // ── CREAR ─────────────────────────────────────────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EquipoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EquipoDto>> Crear([FromBody] CrearEquipoDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var usuario = ObtenerUsuarioActual();
        var creado = await _servicio.CrearAsync(dto, usuario);
        _log.LogInformation("Equipo creado {Id} por {Usuario}", creado.Id, usuario);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    // ── ACTUALIZAR ────────────────────────────────────────────────────────────
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EquipoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EquipoDto>> Actualizar(int id, [FromBody] ActualizarEquipoDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        try
        {
            var usuario = ObtenerUsuarioActual();
            var actualizado = await _servicio.ActualizarAsync(id, dto, usuario);
            _log.LogInformation("Equipo {Id} actualizado por {Usuario}", id, usuario);
            return Ok(actualizado);
        }
        catch (NotFoundException ex)
        {
            return NotFound(CrearProblemDetails(404, "Recurso no encontrado", ex.Message));
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(CrearProblemDetails(409, "Conflicto de regla", ex.Message));
        }
    }

    // ── BAJA LÓGICA ───────────────────────────────────────────────────────────
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DarDeBaja(int id)
    {
        try
        {
            var usuario = ObtenerUsuarioActual();
            await _servicio.DarDeBajaAsync(id, usuario);
            _log.LogInformation("Equipo {Id} dado de baja lógica por {Usuario}", id, usuario);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(CrearProblemDetails(404, "Recurso no encontrado", ex.Message));
        }
    }

    // ── ASIGNAR ───────────────────────────────────────────────────────────────
    [HttpPost("{id:int}/asignar")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EquipoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EquipoDto>> Asignar(int id, [FromBody] AsignarEquipoDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        try
        {
            var usuario = ObtenerUsuarioActual();
            var asignado = await _servicio.AsignarAReporteAsync(id, dto, usuario);
            return Ok(asignado);
        }
        catch (NotFoundException ex)
        {
            return NotFound(CrearProblemDetails(404, "Recurso no encontrado", ex.Message));
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(CrearProblemDetails(409, "Regla de asignación infringida", ex.Message));
        }
    }

    // ── LIBERAR ───────────────────────────────────────────────────────────────
    [HttpPost("{id:int}/liberar")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EquipoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EquipoDto>> Liberar(int id)
    {
        try
        {
            var usuario = ObtenerUsuarioActual();
            var liberado = await _servicio.LiberarDeReporteAsync(id, usuario);
            return Ok(liberado);
        }
        catch (NotFoundException ex)
        {
            return NotFound(CrearProblemDetails(404, "Recurso no encontrado", ex.Message));
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(CrearProblemDetails(409, "Regla de negocio infringida", ex.Message));
        }
    }

    // ── HELPERS ───────────────────────────────────────────────────────────────
    private string ObtenerUsuarioActual()
    {
        var claim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                 ?? User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                 ?? User?.Identity?.Name;

        if (!string.IsNullOrWhiteSpace(claim)) return claim;

        if (Request.Headers.TryGetValue("X-Id-Usuario", out var hdr) && !string.IsNullOrWhiteSpace(hdr))
            return hdr.ToString();

        return "admin@ciudad.gob";
    }

    private static ProblemDetails CrearProblemDetails(int codigo, string titulo, string detalle) =>
        new()
        {
            Status = codigo,
            Title = titulo,
            Detail = detalle,
            Instance = $"api/equipos/{codigo}"
        };
}
