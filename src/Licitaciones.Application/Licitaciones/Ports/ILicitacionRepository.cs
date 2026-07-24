using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.Licitaciones.Ports;

public interface ILicitacionRepository
{
    Task<bool> ExisteCodigoNormalizadoAsync(
        string codigoNormalizado,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Licitacion licitacion,
        CancellationToken cancellationToken = default);
}
