namespace Licitaciones.Application.Ofertas.Exceptions;

public sealed class OfertaNoEncontradaException(string message)
    : Exception(message);
