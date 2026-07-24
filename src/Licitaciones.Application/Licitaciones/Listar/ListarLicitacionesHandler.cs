using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Licitaciones.Ports;

namespace Licitaciones.Application.Licitaciones.Listar;

public sealed class ListarLicitacionesHandler(
    ILicitacionReadRepository repository,
    IClock clock)
{
    private const int PageSizeDefault = 10;
    private const int PageSizeMaximum = 100;

    public Task<PaginaResultado<LicitacionListadoDto>> HandleAsync(
        ListarLicitacionesQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(query.Page, 1);
        var pageSize = query.PageSize <= 0
            ? PageSizeDefault
            : Math.Min(query.PageSize, PageSizeMaximum);
        var search = string.IsNullOrWhiteSpace(query.Search)
            ? null
            : query.Search.Trim().ToUpperInvariant();
        var sortBy = query.SortBy?.ToLowerInvariant() switch
        {
            "fecha_cierre_desc" => OrdenLicitacion.FechaCierreDescendente,
            "codigo" => OrdenLicitacion.CodigoAscendente,
            "codigo_desc" => OrdenLicitacion.CodigoDescendente,
            _ => OrdenLicitacion.FechaCierreAscendente
        };

        return repository.ListarAsync(
            new LicitacionesConsulta(
                page,
                pageSize,
                search,
                query.FiltroEstado,
                query.FechaDesde,
                query.FechaHasta,
                sortBy),
            clock.UtcNow,
            cancellationToken);
    }
}
