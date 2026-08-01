namespace Licitaciones.Domain.Exceptions;

public sealed class TransicionEstadoInvalidaException(string mensaje)
    : InvalidOperationException(mensaje);
