using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.Proveedores.Ports;

public interface IProveedorEditRepository
{
    Task<ProveedorEdicion?> ObtenerPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteNombreNormalizadoAsync(
        string nombreNormalizado,
        Guid excluirProveedorId,
        CancellationToken cancellationToken = default);

    Task GuardarAsync(
        Proveedor proveedor,
        int expectedRowVersion,
        CancellationToken cancellationToken = default);
}

public sealed record ProveedorEdicion(
    Proveedor Proveedor,
    int RowVersion);

