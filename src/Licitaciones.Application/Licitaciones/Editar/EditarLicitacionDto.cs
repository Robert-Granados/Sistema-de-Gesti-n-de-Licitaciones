namespace Licitaciones.Application.Licitaciones.Editar;

public sealed record EditarLicitacionDto(
    Guid Id,
    string Codigo,
    string Titulo,
    DateTimeOffset FechaCierre,
    decimal PresupuestoEstimadoCrc,
    int RowVersion);
