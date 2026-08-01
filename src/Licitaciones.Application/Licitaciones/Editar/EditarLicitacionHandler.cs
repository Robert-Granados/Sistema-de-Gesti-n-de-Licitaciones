using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Licitaciones.Exceptions;
using Licitaciones.Application.Licitaciones.Ports;
using Licitaciones.Domain.Enums;

namespace Licitaciones.Application.Licitaciones.Editar;

public sealed class EditarLicitacionHandler(
    ILicitacionEditRepository repository,
    IClock clock)
{
    public async Task<EditarLicitacionDto?> ObtenerAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var licitacion = await repository.ObtenerPorIdAsync(id, cancellationToken);

        return licitacion is null
            ? null
            : new EditarLicitacionDto(
                licitacion.Licitacion.Id,
                licitacion.Licitacion.Codigo,
                licitacion.Licitacion.Titulo,
                licitacion.Licitacion.FechaCierre,
                licitacion.Licitacion.PresupuestoEstimadoCrc,
                licitacion.RowVersion);
    }

    public async Task HandleAsync(
        EditarLicitacionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var licitacion = await repository.ObtenerPorIdAsync(
            command.Id,
            cancellationToken);

        if (licitacion is null)
        {
            throw new LicitacionNoEncontradaException(
                "La licitación no fue encontrada.");
        }

        if (licitacion.Licitacion.Estado == EstadoLicitacion.Cerrada)
        {
            throw new LicitacionCerradaException(
                "No se puede editar una licitación cerrada.");
        }

        if (licitacion.Licitacion.FechaCierre <= clock.UtcNow)
        {
            throw new LicitacionCerradaException(
                "No se puede editar una licitación cuya fecha de cierre ya pasó.");
        }

        var maxMontoOfertado = await repository.ObtenerMaxMontoOfertadoAsync(
            command.Id,
            cancellationToken);

        if (command.PresupuestoEstimadoCrc < maxMontoOfertado)
        {
            throw new PresupuestoInsuficienteException(
                $"El presupuesto no puede ser menor al monto de la mayor oferta registrada ({maxMontoOfertado:N2} CRC).");
        }

        if (command.FechaCierreUtc <= clock.UtcNow)
        {
            throw new ArgumentOutOfRangeException(
                nameof(command),
                "La fecha de cierre debe ser futura.");
        }

        licitacion.Licitacion.CambiarTitulo(command.Titulo);
        licitacion.Licitacion.ActualizarFechaCierre(command.FechaCierreUtc);
        licitacion.Licitacion.ActualizarPresupuesto(command.PresupuestoEstimadoCrc);

        await repository.GuardarAsync(
            licitacion.Licitacion,
            command.RowVersion,
            cancellationToken);
    }
}
