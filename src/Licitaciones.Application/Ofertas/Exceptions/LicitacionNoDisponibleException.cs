namespace Licitaciones.Application.Ofertas.Exceptions;

public sealed class LicitacionNoDisponibleException(string message)
    : Exception(message);
