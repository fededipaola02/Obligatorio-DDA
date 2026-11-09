using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Obligatorio.Api.DTOs;
using Obligatorio.Api.Interfaces;
using Obligatorio.Api.Models.Entities;
using Obligatorio.Api.Models.Enums;
using Obligatorio.Api.Data;
using Obligatorio.Api.Exceptions;

namespace Obligatorio.Api.Services;

public class EquipoService : IEquipoService
{
    private readonly IEquipoRepository _repo;
    private readonly AppDbContext _ctx;

    public EquipoService(IEquipoRepository repo, AppDbContext ctx)
    {
        _repo = repo;
        _ctx = ctx;
    }

    private static EquipoDto Map(Equipo e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        Tipo = e.Tipo,
        Estado = e.Estado,
        Activo = e.Activo,
        ReporteActualId = e.ReporteActualId,
        TituloReporteActual = e.ReporteActual?.Titulo,
        FechaCreacion = e.FechaCreacion,
        FechaActualizacion = e.FechaActualizacion
    };

    public async Task<List<EquipoDto>> ObtenerTodosAsync(FiltroEquipoDto filtro) =>
        (await _repo.ObtenerTodosAsync(filtro)).Select(Map).ToList();

    public async Task<EquipoDto> ObtenerPorIdAsync(int id)
    {
        var e = await _repo.ObtenerPorIdAsync(id)
            ?? throw new NotFoundException("Equipo no encontrado");
        return Map(e);
    }

    public async Task<EquipoDto> CrearAsync(CrearEquipoDto dto, string usuarioId)
    {
        var entidad = new Equipo
        {
            Nombre = dto.Nombre,
            Tipo = dto.Tipo,
            Estado = dto.Estado,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
        await _repo.CrearAsync(entidad);
        await RegistrarAuditoria(entidad.Id, "Crear", null, entidad.Estado, usuarioId);
        return Map(entidad);
    }

    public async Task<EquipoDto> ActualizarAsync(int id, ActualizarEquipoDto dto, string usuarioId)
    {
        var entidad = await _repo.ObtenerPorIdAsync(id)
            ?? throw new NotFoundException("Equipo no encontrado");

        var estadoPrevio = entidad.Estado;
        entidad.Nombre = dto.Nombre;
        entidad.Tipo = dto.Tipo;
        entidad.Estado = dto.Estado;
        entidad.FechaActualizacion = DateTime.UtcNow;

        await _repo.ActualizarAsync(entidad);
        await RegistrarAuditoria(id, "Actualizar", estadoPrevio, entidad.Estado, usuarioId);
        return Map(entidad);
    }

    public async Task DarDeBajaAsync(int id, string usuarioId)
    {
        var entidad = await _repo.ObtenerPorIdAsync(id)
            ?? throw new NotFoundException("Equipo no encontrado");

        if (!entidad.Activo) return;

        await _repo.BorrarLogicamenteAsync(id);
        await RegistrarAuditoria(id, "Baja Lógica", entidad.Estado, null, usuarioId);
    }

    public async Task<EquipoDto> AsignarAReporteAsync(int id, AsignarEquipoDto dto, string usuarioId)
    {
        var entidad = await _repo.ObtenerPorIdAsync(id)
            ?? throw new NotFoundException("Equipo no encontrado");

        if (entidad.Estado == EstadoEquipo.EnUso || entidad.Estado == EstadoEquipo.EnMantenimiento)
            throw new ReglaNegocioException("El equipo está en uso o en mantenimiento y no puede asignarse.");

        await _repo.AsignarAReporteAsync(id, dto.ReporteId);
        await RegistrarAuditoria(id, "Asignar", entidad.Estado, EstadoEquipo.EnUso, usuarioId);

        var actualizado = await _repo.ObtenerPorIdAsync(id);
        return Map(actualizado!);
    }

    public async Task<EquipoDto> LiberarDeReporteAsync(int id, string usuarioId)
    {
        var entidad = await _repo.ObtenerPorIdAsync(id)
            ?? throw new NotFoundException("Equipo no encontrado");

        await _repo.LiberarDeReporteAsync(id);
        await RegistrarAuditoria(id, "Liberar", entidad.Estado, EstadoEquipo.Disponible, usuarioId);

        var actualizado = await _repo.ObtenerPorIdAsync(id);
        return Map(actualizado!);
    }

    // ── AUDITORÍA ─────────────────────────────────────────────────────────────
    private async Task RegistrarAuditoria(int equipoId, string accion,
        EstadoEquipo? estadoAnterior, EstadoEquipo? estadoNuevo, string usuarioId)
    {
        var registro = new AuditoriaEquipo
        {
            EquipoId = equipoId,
            Accion = accion,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = estadoNuevo,
            UsuarioId = usuarioId,
            FechaHora = DateTime.UtcNow
        };
        _ctx.AuditoriasEquipos.Add(registro);
        await _ctx.SaveChangesAsync();
    }
}
