using Licitaciones.Domain.Enums;

namespace Licitaciones.Application.Licitaciones.Listar;

public sealed record LicitacionListadoDto(
    Guid Id,
    string Codigo,
    string Titulo,
    EstadoLicitacion Estado,
    DateTimeOffset FechaCierre,
    decimal PresupuestoEstimadoCrc,
    bool EstaCerradaFuncionalmente);
