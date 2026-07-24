using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.Proveedores.Ports;

public interface IProveedorDeleteRepository
{
    Task<Proveedor?> ObtenerActivoPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> TieneOfertasAsync(
        Guid proveedorId,
        CancellationToken cancellationToken = default);

    Task GuardarBorradoLogicoAsync(
        Proveedor proveedor,
        CancellationToken cancellationToken = default);
}
