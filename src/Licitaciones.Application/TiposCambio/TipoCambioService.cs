using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.TiposCambio;

public sealed class TipoCambioService(ITipoCambioRepository repository)
{
    public async Task<IReadOnlyList<TipoCambioDto>> ListarAsync(
        CancellationToken cancellationToken = default) =>
        (await repository.ListarAsync(cancellationToken))
        .Select(Mapear)
        .ToList();

    public async Task<TipoCambioDto?> ObtenerAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tipoCambio = await repository.ObtenerAsync(id, cancellationToken);
        return tipoCambio is null ? null : Mapear(tipoCambio);
    }

    public async Task<TipoCambioDto> CrearAsync(
        decimal crcPorUsd,
        DateTimeOffset fechaVigencia,
        bool activar = false,
        CancellationToken cancellationToken = default)
    {
        var tipoCambio = new TipoCambio(crcPorUsd, fechaVigencia);
        await repository.AgregarAsync(tipoCambio, cancellationToken);

        if (activar)
        {
            await repository.ActivarEnTransaccionAsync(tipoCambio, cancellationToken);
        }

        return Mapear(tipoCambio);
    }

    public async Task<TipoCambioDto> EditarAsync(
        Guid id,
        decimal crcPorUsd,
        DateTimeOffset fechaVigencia,
        CancellationToken cancellationToken = default)
    {
        var tipoCambio = await ObtenerEntidadAsync(id, cancellationToken);
        tipoCambio.Actualizar(crcPorUsd, fechaVigencia);
        await repository.GuardarAsync(cancellationToken);
        return Mapear(tipoCambio);
    }

    public async Task<TipoCambioDto> ActivarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tipoCambio = await ObtenerEntidadAsync(id, cancellationToken);
        await repository.ActivarEnTransaccionAsync(tipoCambio, cancellationToken);
        return Mapear(tipoCambio);
    }

    public async Task EliminarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tipoCambio = await ObtenerEntidadAsync(id, cancellationToken);
        await repository.EliminarAsync(tipoCambio, cancellationToken);
    }

    private async Task<TipoCambio> ObtenerEntidadAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await repository.ObtenerAsync(id, cancellationToken)
        ?? throw new KeyNotFoundException("El tipo de cambio no existe.");

    private static TipoCambioDto Mapear(TipoCambio tipoCambio) =>
        new(
            tipoCambio.Id,
            tipoCambio.CrcPorUsd,
            tipoCambio.FechaVigencia,
            tipoCambio.Activo);
}
