using System.Collections.Generic;
using System.Threading.Tasks;
using Obligatorio.Api.DTOs;
using Obligatorio.Api.Models.Entities;
using Obligatorio.Api.Models.Enums;

namespace Obligatorio.Api.Interfaces;

/// <summary>
/// Operaciones de acceso a datos para Equipo.
/// </summary>
public interface IEquipoRepository
{
    Task<List<Equipo>> ObtenerTodosAsync(FiltroEquipoDto filtro);
    Task<Equipo?> ObtenerPorIdAsync(int id);
    Task<Equipo> CrearAsync(Equipo entidad);
    Task<Equipo> ActualizarAsync(Equipo entidad);
    Task BorrarLogicamenteAsync(int id);
    Task AsignarAReporteAsync(int id, int reporteId);
    Task LiberarDeReporteAsync(int id);
}
