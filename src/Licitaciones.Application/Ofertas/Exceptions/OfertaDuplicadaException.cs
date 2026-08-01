namespace Licitaciones.Application.Ofertas.Exceptions;

public sealed class OfertaDuplicadaException(string message)
    : Exception(message);
