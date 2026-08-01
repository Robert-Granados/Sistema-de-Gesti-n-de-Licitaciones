using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Licitaciones.Listar;

namespace Licitaciones.Web.Models.Licitaciones;

public sealed record ListarLicitacionesViewModel(
    PaginaResultado<LicitacionListadoDto> Resultado,
    string? Search,
    string? FiltroEstado,
    string? FechaDesde,
    string? FechaHasta,
    string SortBy,
    int PageSize);
