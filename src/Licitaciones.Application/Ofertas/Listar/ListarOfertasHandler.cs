using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Ofertas.Ports;

namespace Licitaciones.Application.Ofertas.Listar;

public sealed class ListarOfertasHandler(IOfertaReadRepository repository)
{
    private const int PageSizeDefault = 10;
    private const int PageSizeMaximum = 100;

    public Task<PaginaResultado<OfertaListadoDto>> HandleAsync(
        ListarOfertasQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(query.Page, 1);
        var pageSize = query.PageSize <= 0
            ? PageSizeDefault
            : Math.Min(query.PageSize, PageSizeMaximum);
        var sortBy = query.SortBy?.ToLowerInvariant() switch
        {
            "monto_desc" => OrdenOferta.MontoDescendente,
            "fecha" => OrdenOferta.FechaAscendente,
            "fecha_desc" => OrdenOferta.FechaDescendente,
            _ => OrdenOferta.MontoAscendente
        };

        return repository.ListarAsync(
            new OfertasConsulta(
                page,
                pageSize,
                NormalizarId(query.LicitacionId),
                NormalizarId(query.ProveedorId),
                sortBy),
            cancellationToken);
    }

    private static Guid? NormalizarId(Guid? id) =>
        id is null || id == Guid.Empty ? null : id;
}
