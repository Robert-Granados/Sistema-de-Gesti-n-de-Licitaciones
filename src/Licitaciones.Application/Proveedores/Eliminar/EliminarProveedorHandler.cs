using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Proveedores.Exceptions;
using Licitaciones.Application.Proveedores.Ports;

namespace Licitaciones.Application.Proveedores.Eliminar;

public sealed class EliminarProveedorHandler(
    IProveedorDeleteRepository repository,
    IClock clock)
{
    public async Task<EliminarProveedorResult> HandleAsync(
        EliminarProveedorCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Id == Guid.Empty)
        {
            throw new ProveedorNoEncontradoException(
                "El proveedor indicado no existe.");
        }

        var proveedor = await repository.ObtenerActivoPorIdAsync(
            command.Id,
            cancellationToken);

        if (proveedor is null)
        {
            throw new ProveedorNoEncontradoException(
                "El proveedor indicado no existe o ya fue eliminado.");
        }

        var tieneOfertas = await repository.TieneOfertasAsync(
            command.Id,
            cancellationToken);

        proveedor.Eliminar(clock.UtcNow);
        await repository.GuardarBorradoLogicoAsync(
            proveedor,
            cancellationToken);

        return new EliminarProveedorResult(proveedor.Id, tieneOfertas);
    }
}
