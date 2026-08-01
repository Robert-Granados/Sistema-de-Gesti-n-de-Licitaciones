namespace Licitaciones.Application.Licitaciones.Editar;

public sealed record EditarLicitacionCommand(
    Guid Id,
    string Titulo,
    DateTimeOffset FechaCierreUtc,
    decimal PresupuestoEstimadoCrc,
    int RowVersion);
