using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Licitaciones.Exceptions;
using Licitaciones.Application.Licitaciones.Ports;

namespace Licitaciones.Application.Licitaciones.Publicar;

public sealed class PublicarLicitacionHandler(
    ILicitacionEditRepository repository,
    IClock clock)
{
    public async Task<PublicarLicitacionResult> HandleAsync(
        PublicarLicitacionCommand command,
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

        licitacion.Licitacion.Publicar(clock.UtcNow);

        await repository.GuardarAsync(
            licitacion.Licitacion,
            licitacion.RowVersion,
            cancellationToken);

        return new PublicarLicitacionResult(
            licitacion.Licitacion.Id,
            licitacion.Licitacion.Codigo);
    }
}
