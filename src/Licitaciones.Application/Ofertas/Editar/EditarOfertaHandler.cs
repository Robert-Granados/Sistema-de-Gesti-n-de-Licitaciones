using Licitaciones.Application.Ofertas.Common;
using Licitaciones.Application.Ofertas.Exceptions;
using Licitaciones.Application.Ofertas.Ports;

namespace Licitaciones.Application.Ofertas.Editar;

public sealed class EditarOfertaHandler(
    IOfertaReadRepository readRepository,
    IOfertaWriteRepository writeRepository,
    OfertaValidador validador)
{
    public Task<EditarOfertaDto?> ObtenerAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Task.FromResult<EditarOfertaDto?>(null);
        }

        return readRepository.ObtenerParaEdicionAsync(
            id,
            cancellationToken);
    }

    public async Task HandleAsync(
        EditarOfertaCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var oferta = await writeRepository.ObtenerPorIdAsync(
            command.Id,
            cancellationToken);

        if (oferta is null)
        {
            throw new OfertaNoEncontradaException(
                "La oferta indicada no existe.");
        }

        // HU-23: se revalidan las reglas de HU-18 (monto positivo,
        // <= presupuesto, licitación publicada y no vencida).
        await validador.ValidarLicitacionAbiertaAsync(
            oferta.LicitacionId,
            "editar",
            cancellationToken);

        await validador.ValidarMontoAsync(
            command.MontoOfertadoCrc,
            oferta.LicitacionId,
            cancellationToken);

        oferta.ActualizarMonto(command.MontoOfertadoCrc);

        await writeRepository.ActualizarAsync(oferta, cancellationToken);
    }
}
