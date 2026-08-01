namespace Licitaciones.Application.Licitaciones.Exceptions;

public sealed class LicitacionConcurrenciaException(string message)
    : Exception(message);
