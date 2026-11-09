using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Obligatorio.Api.DTOs;
using Obligatorio.Api.Interfaces;
using Obligatorio.Api.Models.Entities;
using Obligatorio.Api.Models.Enums;
using Obligatorio.Api.Data;

namespace Obligatorio.Api.Repositories;

public class EquipoRepository : IEquipoRepository
{
    private readonly AppDbContext _context;

    public EquipoRepository(AppDbContext context) => _context = context;

    public async Task<List<Equipo>> ObtenerTodosAsync(FiltroEquipoDto filtro)
    {
        var query = _context.Equipos.AsQueryable();

        if (filtro.SoloActivos)
            query = query.Where(e => e.Activo);

        if (filtro.Tipo.HasValue)
            query = query.Where(e => e.Tipo == filtro.Tipo.Value);

        if (filtro.Estado.HasValue)
            query = query.Where(e => e.Estado == filtro.Estado.Value);

        if (!string.IsNullOrWhiteSpace(filtro.Buscar))
        {
            var term = filtro.Buscar.Trim().ToLower();
            query = query.Where(e => e.Nombre.ToLower().Contains(term));
        }

        query = filtro.OrdenarPor?.ToLower() switch
        {
            "nombre" => query.OrderBy(e => e.Nombre),
            "tipo"   => query.OrderBy(e => e.Tipo),
            "estado" => query.OrderBy(e => e.Estado),
            _        => query.OrderBy(e => e.Id)
        };

        return await query.ToListAsync();
    }

    public async Task<Equipo?> ObtenerPorIdAsync(int id) =>
        await _context.Equipos.FirstOrDefaultAsync(e => e.Id == id && e.Activo);

    public async Task<Equipo> CrearAsync(Equipo entidad)
    {
        _context.Equipos.Add(entidad);
        await _context.SaveChangesAsync();
        return entidad;
    }

    public async Task<Equipo> ActualizarAsync(Equipo entidad)
    {
        _context.Equipos.Update(entidad);
        await _context.SaveChangesAsync();
        return entidad;
    }

    public async Task BorrarLogicamenteAsync(int id)
    {
        var equipo = await ObtenerPorIdAsync(id);
        if (equipo == null) return;
        equipo.Activo = false;
        await _context.SaveChangesAsync();
    }

    public async Task AsignarAReporteAsync(int id, int reporteId)
    {
        var equipo = await ObtenerPorIdAsync(id);
        if (equipo == null) return;
        equipo.Estado = EstadoEquipo.EnUso;
        equipo.ReporteActualId = reporteId;
        await _context.SaveChangesAsync();
    }

    public async Task LiberarDeReporteAsync(int id)
    {
        var equipo = await ObtenerPorIdAsync(id);
        if (equipo == null) return;
        equipo.Estado = EstadoEquipo.Disponible;
        equipo.ReporteActualId = null;
        await _context.SaveChangesAsync();
    }
}
