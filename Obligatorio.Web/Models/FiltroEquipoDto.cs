namespace Obligatorio.Web.Models;

public class FiltroEquipoDto
{
    public TipoEquipo? Tipo { get; set; }
    public EstadoEquipo? Estado { get; set; }
    public string? OrdenarPor { get; set; } = "fecha_desc";
    public string? Buscar { get; set; }
    public bool SoloActivos { get; set; } = true;
}

