using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.TiposCambio;

public interface ITipoCambioRepository
{
    Task<IReadOnlyList<TipoCambio>> ListarAsync(
        CancellationToken cancellationToken = default);
    Task<TipoCambio?> ObtenerAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    Task AgregarAsync(
        TipoCambio tipoCambio,
        CancellationToken cancellationToken = default);
    Task GuardarAsync(CancellationToken cancellationToken = default);
    Task EliminarAsync(
        TipoCambio tipoCambio,
        CancellationToken cancellationToken = default);
    Task ActivarEnTransaccionAsync(
        TipoCambio tipoCambio,
        CancellationToken cancellationToken = default);
}
