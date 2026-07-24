using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Licitaciones.Exceptions;
using Licitaciones.Application.Licitaciones.Ports;
using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.Licitaciones.Crear;

public sealed class CrearLicitacionHandler(
    ILicitacionRepository repository,
    IClock clock)
{
    public async Task<CrearLicitacionResult> HandleAsync(
        CrearLicitacionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.PresupuestoEstimadoCrc <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(command),
                "El presupuesto estimado debe ser mayor que cero.");
        }

        if (command.FechaCierreUtc <= clock.UtcNow)
        {
            throw new ArgumentOutOfRangeException(
                nameof(command),
                "La fecha de cierre debe ser futura.");
        }

        var codigoNormalizado = command.Codigo.Trim().ToUpperInvariant();

        if (await repository.ExisteCodigoNormalizadoAsync(
                codigoNormalizado,
                cancellationToken))
        {
            throw new LicitacionDuplicadaException(
                "Ya existe una licitación registrada con ese código.");
        }

        var licitacion = new Licitacion(
            command.Codigo,
            command.Titulo,
            command.FechaCierreUtc,
            command.PresupuestoEstimadoCrc);

        await repository.AgregarAsync(licitacion, cancellationToken);

        return new CrearLicitacionResult(licitacion.Id, licitacion.Codigo);
    }
}
