namespace Licitaciones.Application.Licitaciones.Crear;

public sealed record CrearLicitacionCommand(
    string Codigo,
    string Titulo,
    DateTimeOffset FechaCierreUtc,
    decimal PresupuestoEstimadoCrc);
