using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Ofertas.Exceptions;
using Licitaciones.Application.Ofertas.Ports;
using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.Ofertas.Registrar;

public sealed class RegistrarOfertaHandler(
    IOfertaValidacionRepository validacionRepository,
    IOfertaWriteRepository writeRepository,
    IClock clock)
{
    public async Task<RegistrarOfertaResult> HandleAsync(
        RegistrarOfertaCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // El orden es parte del contrato de HU-18: existencia, estado,
        // vencimiento, monto y, finalmente, duplicidad.
        var fechaCierre = await validacionRepository.ObtenerFechaCierreAsync(
            command.LicitacionId, cancellationToken);

        if (fechaCierre is null)
        {
            throw new LicitacionNoDisponibleException(
                "La licitación indicada no existe.");
        }

        if (!await validacionRepository.ProveedorExisteAsync(
                command.ProveedorId, cancellationToken))
        {
            throw new ProveedorNoEncontradoException(
                "El proveedor indicado no existe.");
        }

        if (!await validacionRepository.ExisteLicitacionPublicadaAsync(
                command.LicitacionId, cancellationToken))
        {
            throw new LicitacionNoDisponibleException(
                "Solo se pueden registrar ofertas para licitaciones publicadas.");
        }

        if (fechaCierre.Value <= clock.UtcNow)
        {
            throw new LicitacionNoDisponibleException(
                "No se pueden registrar ofertas para licitaciones vencidas.");
        }

        if (command.MontoOfertadoCrc <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(command.MontoOfertadoCrc),
                "El monto ofertado debe ser mayor que cero.");
        }

        var presupuesto = await validacionRepository.ObtenerPresupuestoAsync(
            command.LicitacionId, cancellationToken);

        if (command.MontoOfertadoCrc > presupuesto)
        {
            throw new ArgumentOutOfRangeException(
                nameof(command.MontoOfertadoCrc),
                $"El monto ofertado ({command.MontoOfertadoCrc:N2} CRC) no puede superar el presupuesto estimado ({presupuesto:N2} CRC).");
        }

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
