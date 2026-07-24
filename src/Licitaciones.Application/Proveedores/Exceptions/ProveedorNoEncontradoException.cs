namespace Licitaciones.Application.Proveedores.Exceptions;

public sealed class ProveedorNoEncontradoException(string message)
    : Exception(message);

