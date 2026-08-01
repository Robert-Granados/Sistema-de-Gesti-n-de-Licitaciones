using Licitaciones.Application.Licitaciones.Exceptions;
using Licitaciones.Application.Licitaciones.Ports;

namespace Licitaciones.Application.Licitaciones.Eliminar;

public sealed class EliminarLicitacionHandler(ILicitacionDeleteRepository repository)
{
    public async Task<EliminarLicitacionResult> HandleAsync(
        EliminarLicitacionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Id == Guid.Empty)
        {
            throw new LicitacionNoEncontradaException(
                "La licitación indicada no existe.");
        }

        var licitacion = await repository.ObtenerActivaPorIdAsync(
            command.Id,
            cancellationToken);

        if (licitacion is null)
        {
            throw new LicitacionNoEncontradaException(
                "La licitación indicada no existe o ya fue eliminada.");
        }

        var tieneOfertas = await repository.TieneOfertasAsync(
            command.Id,
            cancellationToken);

        licitacion.Eliminar(DateTimeOffset.UtcNow);
        await repository.GuardarBorradoLogicoAsync(
            licitacion,
            cancellationToken);

        return new EliminarLicitacionResult(licitacion.Id, tieneOfertas);
    }
}
