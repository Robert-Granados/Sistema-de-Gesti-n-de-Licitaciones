namespace Licitaciones.Application.Proveedores.Exceptions;

public sealed class ProveedorConcurrenciaException(string message)
    : Exception(message);

