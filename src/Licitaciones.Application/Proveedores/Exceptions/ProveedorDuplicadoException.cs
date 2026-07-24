namespace Licitaciones.Application.Proveedores.Exceptions;

public sealed class ProveedorDuplicadoException(string message)
    : Exception(message);

