using Licitaciones.Application.Proveedores.Exceptions;
using Licitaciones.Application.Proveedores.Ports;
using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.Proveedores.Crear;

public sealed class CrearProveedorHandler(IProveedorRepository repository)
{
    public async Task<CrearProveedorResult> HandleAsync(
        CrearProveedorCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var nombre = NombreProveedorNormalizer.Limpiar(command.Nombre);

        if (!NombreProveedorNormalizer.EsValido(nombre))
        {
            throw new NombreProveedorInvalidoException(
                "El nombre solo puede contener letras, números, espacios, punto, coma o paréntesis.");
        }

        var nombreNormalizado = NombreProveedorNormalizer.Normalizar(nombre);

        if (await repository.ExisteNombreNormalizadoAsync(
                nombreNormalizado,
                cancellationToken))
        {
            throw new ProveedorDuplicadoException(
                "Ya existe un proveedor registrado con ese nombre.");
        }

        var proveedor = new Proveedor(nombre, nombreNormalizado);
        await repository.AgregarAsync(proveedor, cancellationToken);

        return new CrearProveedorResult(proveedor.Id, proveedor.Nombre);
    }
}

