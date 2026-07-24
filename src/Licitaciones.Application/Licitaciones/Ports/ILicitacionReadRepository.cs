using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Licitaciones.Listar;

namespace Licitaciones.Application.Licitaciones.Ports;

public interface ILicitacionReadRepository
{
    Task<PaginaResultado<LicitacionListadoDto>> ListarAsync(
        LicitacionesConsulta consulta,
        DateTimeOffset ahoraUtc,
        CancellationToken cancellationToken = default);
}

public sealed record LicitacionesConsulta(
    int Page,
    int PageSize,
    string? Search,
    string? FiltroEstado,
    DateTimeOffset? FechaDesde,
    DateTimeOffset? FechaHasta,
    OrdenLicitacion SortBy);

public enum OrdenLicitacion
{
    FechaCierreAscendente,
    FechaCierreDescendente,
    CodigoAscendente,
    CodigoDescendente
}
