namespace Licitaciones.Application.Licitaciones.Exceptions;

public sealed class LicitacionCerradaException(string message)
    : Exception(message);
