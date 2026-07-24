using Licitaciones.Application.Proveedores.Detalle;

namespace Licitaciones.Application.Proveedores.Ports;

public interface IProveedorDetalleRepository
{
    Task<ProveedorDetalleDto?> ObtenerPorIdAsync(
        ProveedorDetalleConsulta consulta,
        CancellationToken cancellationToken = default);
}

public sealed record ProveedorDetalleConsulta(
    Guid Id,
    int Page,
    int PageSize);

