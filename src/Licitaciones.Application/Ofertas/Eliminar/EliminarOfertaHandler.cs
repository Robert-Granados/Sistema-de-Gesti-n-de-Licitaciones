using Licitaciones.Application.Ofertas.Common;
using Licitaciones.Application.Ofertas.Exceptions;
using Licitaciones.Application.Ofertas.Ports;

namespace Licitaciones.Application.Ofertas.Eliminar;

public sealed class EliminarOfertaHandler(
    IOfertaWriteRepository writeRepository,
    OfertaValidador validador)
{
    public async Task<EliminarOfertaResult> HandleAsync(
        EliminarOfertaCommand command,
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

        // HU-24: se rechaza la eliminación si la licitación está
        // cerrada formal o funcionalmente.
        await validador.ValidarLicitacionAbiertaAsync(
            oferta.LicitacionId,
            "eliminar",
            cancellationToken);

        await writeRepository.EliminarAsync(oferta, cancellationToken);

        return new EliminarOfertaResult(oferta.Id);
    }
}
