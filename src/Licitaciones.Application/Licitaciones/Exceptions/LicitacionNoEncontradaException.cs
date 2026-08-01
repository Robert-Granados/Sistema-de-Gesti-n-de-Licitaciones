namespace Licitaciones.Application.Licitaciones.Exceptions;

public sealed class LicitacionNoEncontradaException(string message)
    : Exception(message);
