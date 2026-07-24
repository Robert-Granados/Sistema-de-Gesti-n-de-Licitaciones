namespace Licitaciones.Application.Licitaciones.Exceptions;

public sealed class LicitacionDuplicadaException(string message)
    : Exception(message);
