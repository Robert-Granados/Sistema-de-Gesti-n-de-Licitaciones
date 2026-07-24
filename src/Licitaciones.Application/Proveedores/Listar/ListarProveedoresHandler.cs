using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Proveedores.Crear;
using Licitaciones.Application.Proveedores.Ports;

namespace Licitaciones.Application.Proveedores.Listar;

public sealed class ListarProveedoresHandler(IProveedorReadRepository repository)
{
    private const int PageSizeDefault = 10;
    private const int PageSizeMaximum = 100;

    public Task<PaginaResultado<ProveedorListadoDto>> HandleAsync(
        ListarProveedoresQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(query.Page, 1);
        var pageSize = query.PageSize <= 0
            ? PageSizeDefault
            : Math.Min(query.PageSize, PageSizeMaximum);
        var search = string.IsNullOrWhiteSpace(query.Search)
            ? null
            : NombreProveedorNormalizer.Normalizar(query.Search);
        var sortBy = query.SortBy?.ToLowerInvariant() switch
        {
            "nombre_desc" => OrdenProveedor.NombreDescendente,
            "id" => OrdenProveedor.IdAscendente,
            "id_desc" => OrdenProveedor.IdDescendente,
            _ => OrdenProveedor.NombreAscendente
        };

        return repository.ListarAsync(
            new ProveedoresConsulta(page, pageSize, search, sortBy),
            cancellationToken);
    }
}

