namespace Licitaciones.Application.Licitaciones.Exceptions;

public sealed class PresupuestoInsuficienteException(string message)
    : Exception(message);
