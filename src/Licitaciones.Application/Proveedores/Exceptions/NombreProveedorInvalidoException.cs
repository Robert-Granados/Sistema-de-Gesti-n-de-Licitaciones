namespace Licitaciones.Application.Proveedores.Exceptions;

public sealed class NombreProveedorInvalidoException(string message)
    : Exception(message);

