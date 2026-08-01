using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.Licitaciones.Ports;

public interface ILicitacionEditRepository
{
    Task<LicitacionEdicion?> ObtenerPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<decimal> ObtenerMaxMontoOfertadoAsync(
        Guid licitacionId,
        CancellationToken cancellationToken = default);

    Task GuardarAsync(
        Licitacion licitacion,
        int expectedRowVersion,
        CancellationToken cancellationToken = default);
}

public sealed record LicitacionEdicion(
    Licitacion Licitacion,
    int RowVersion);
