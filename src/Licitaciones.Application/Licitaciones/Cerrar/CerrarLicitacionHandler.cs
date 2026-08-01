using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Licitaciones.Exceptions;
using Licitaciones.Application.Licitaciones.Ports;

namespace Licitaciones.Application.Licitaciones.Cerrar;

public sealed class CerrarLicitacionHandler(
    ILicitacionEditRepository repository,
    IClock clock)
{
    public async Task<CerrarLicitacionResult> HandleAsync(
        CerrarLicitacionCommand command,
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

        if (licitacion.Licitacion.Estado == Domain.Enums.EstadoLicitacion.Cerrada)
        {
            throw new LicitacionCerradaException(
                "La licitación ya está cerrada.");
        }

        licitacion.Licitacion.Cerrar(command.Motivo, clock.UtcNow);

        await repository.GuardarAsync(
            licitacion.Licitacion,
            licitacion.RowVersion,
            cancellationToken);

        return new CerrarLicitacionResult(
            licitacion.Licitacion.Id,
            licitacion.Licitacion.Codigo);
    }
}
