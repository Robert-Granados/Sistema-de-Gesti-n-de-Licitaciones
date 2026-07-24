using Licitaciones.Application.Proveedores.Crear;
using Licitaciones.Application.Proveedores.Exceptions;
using Licitaciones.Application.Proveedores.Ports;

namespace Licitaciones.Application.Proveedores.Editar;

public sealed class EditarProveedorHandler(IProveedorEditRepository repository)
{
    public async Task<EditarProveedorDto?> ObtenerAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var proveedor = await repository.ObtenerPorIdAsync(id, cancellationToken);

        return proveedor is null
            ? null
            : new EditarProveedorDto(
                proveedor.Proveedor.Id,
                proveedor.Proveedor.Nombre,
                proveedor.RowVersion);
    }

    public async Task HandleAsync(
        EditarProveedorCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var nombre = NombreProveedorNormalizer.Limpiar(command.Nombre);

        if (!NombreProveedorNormalizer.EsValido(nombre))
        {
            throw new NombreProveedorInvalidoException(
                "El nombre solo puede contener letras, números, espacios, punto, coma o paréntesis.");
        }

        var proveedor = await repository.ObtenerPorIdAsync(
            command.Id,
            cancellationToken);

        if (proveedor is null)
        {
            throw new ProveedorNoEncontradoException(
                "El proveedor no existe o fue eliminado.");
        }

        var nombreNormalizado = NombreProveedorNormalizer.Normalizar(nombre);
        var existeDuplicado = await repository.ExisteNombreNormalizadoAsync(
            nombreNormalizado,
            command.Id,
            cancellationToken);

        if (existeDuplicado)
        {
            throw new ProveedorDuplicadoException(
                "Ya existe otro proveedor registrado con ese nombre.");
        }

        proveedor.Proveedor.CambiarNombre(nombre, nombreNormalizado);
        await repository.GuardarAsync(
            proveedor.Proveedor,
            command.RowVersion,
            cancellationToken);
    }
}

