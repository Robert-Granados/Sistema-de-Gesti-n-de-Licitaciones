using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Ofertas.Common;
using Licitaciones.Application.Ofertas.Exceptions;
using Licitaciones.Application.Ofertas.Ports;
using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.Ofertas.Registrar;

public sealed class RegistrarOfertaHandler(
    IOfertaValidacionRepository validacionRepository,
    IOfertaWriteRepository writeRepository,
    IClock clock)
{
    private readonly OfertaValidador _validador = new(validacionRepository, clock);

    public async Task<RegistrarOfertaResult> HandleAsync(
        RegistrarOfertaCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // El orden es parte del contrato de HU-18: existencia, estado,
        // vencimiento, monto y, finalmente, duplicidad.
        if (!await validacionRepository.ProveedorExisteAsync(
                command.ProveedorId, cancellationToken))
        {
            throw new ProveedorNoEncontradoException(
                "El proveedor indicado no existe.");
        }

        await _validador.ValidarLicitacionAbiertaAsync(
            command.LicitacionId,
            "registrar",
            cancellationToken);

        await _validador.ValidarMontoAsync(
            command.MontoOfertadoCrc,
            command.LicitacionId,
            cancellationToken);

        if (await validacionRepository.YaTieneOfertaAsync(
                command.LicitacionId, command.ProveedorId, cancellationToken))
        {
            throw new OfertaDuplicadaException(
                "Este proveedor ya tiene una oferta registrada para esta licitación.");
        }

        var oferta = new Oferta(
            command.LicitacionId,
            command.ProveedorId,
            command.MontoOfertadoCrc,
            clock.UtcNow);

        await writeRepository.AgregarAsync(oferta, cancellationToken);

        return new RegistrarOfertaResult(
            oferta.Id,
            oferta.LicitacionId,
            oferta.ProveedorId);
    }
}
