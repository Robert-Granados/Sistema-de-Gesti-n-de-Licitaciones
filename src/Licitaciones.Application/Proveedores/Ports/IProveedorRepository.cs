using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.Proveedores.Ports;

public interface IProveedorRepository
{
    Task<bool> ExisteNombreNormalizadoAsync(
        string nombreNormalizado,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Proveedor proveedor,
        CancellationToken cancellationToken = default);
}

