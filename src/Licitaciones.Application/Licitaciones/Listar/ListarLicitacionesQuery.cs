namespace Licitaciones.Application.Licitaciones.Listar;

public sealed record ListarLicitacionesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? FiltroEstado = null,
    DateTimeOffset? FechaDesde = null,
    DateTimeOffset? FechaHasta = null,
    string? SortBy = null);
