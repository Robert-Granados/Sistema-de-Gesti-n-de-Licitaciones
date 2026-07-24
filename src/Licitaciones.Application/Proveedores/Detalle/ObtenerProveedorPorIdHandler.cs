using Licitaciones.Application.Proveedores.Ports;

namespace Licitaciones.Application.Proveedores.Detalle;

public sealed class ObtenerProveedorPorIdHandler(
    IProveedorDetalleRepository repository)
{
    private const int PageSizeDefault = 10;
    private const int PageSizeMaximum = 100;

    public Task<ProveedorDetalleDto?> HandleAsync(
        ObtenerProveedorPorIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Id == Guid.Empty)
        {
            return Task.FromResult<ProveedorDetalleDto?>(null);
        }

        var page = Math.Max(query.Page, 1);
        var pageSize = query.PageSize <= 0
            ? PageSizeDefault
            : Math.Min(query.PageSize, PageSizeMaximum);

        return repository.ObtenerPorIdAsync(
            new ProveedorDetalleConsulta(query.Id, page, pageSize),
            cancellationToken);
    }
}

