using System.Collections.Generic;
using System.Threading.Tasks;
using Obligatorio.Api.DTOs;

namespace Obligatorio.Api.Interfaces;

/// <summary>
/// Lógica de negocio del recurso Equipo.
/// </summary>
public interface IEquipoService
{
    Task<List<EquipoDto>> ObtenerTodosAsync(FiltroEquipoDto filtro);
    Task<EquipoDto> ObtenerPorIdAsync(int id);
    Task<EquipoDto> CrearAsync(CrearEquipoDto dto, string usuarioId);
    Task<EquipoDto> ActualizarAsync(int id, ActualizarEquipoDto dto, string usuarioId);
    Task DarDeBajaAsync(int id, string usuarioId);
    Task<EquipoDto> AsignarAReporteAsync(int id, AsignarEquipoDto dto, string usuarioId);
    Task<EquipoDto> LiberarDeReporteAsync(int id, string usuarioId);
}
