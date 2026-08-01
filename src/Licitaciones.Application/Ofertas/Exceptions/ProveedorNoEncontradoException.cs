namespace Licitaciones.Application.Ofertas.Exceptions;

public sealed class ProveedorNoEncontradoException(string message)
    : Exception(message);
