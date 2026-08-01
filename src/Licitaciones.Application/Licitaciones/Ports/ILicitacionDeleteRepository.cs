using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.Licitaciones.Ports;

public interface ILicitacionDeleteRepository
{
    Task<Licitacion?> ObtenerActivaPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> TieneOfertasAsync(
        Guid licitacionId,
        CancellationToken cancellationToken = default);

    Task GuardarBorradoLogicoAsync(
        Licitacion licitacion,
        CancellationToken cancellationToken = default);
}
