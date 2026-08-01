using Licitaciones.Application.Licitaciones.Exceptions;
using Licitaciones.Application.Licitaciones.Ports;
using Licitaciones.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

internal sealed class LicitacionEditRepository(AppDbContext dbContext)
    : ILicitacionEditRepository
{
    private const string ConcurrencyConstraintName =
        "ix_licitaciones_row_version";

    public async Task<LicitacionEdicion?> ObtenerPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var licitacion = await dbContext.Licitaciones
            .FindAsync([id], cancellationToken);

        if (licitacion is null)
        {
            return null;
        }

        var rowVersion = dbContext.Entry(licitacion)
            .Property<int>("RowVersion")
            .CurrentValue;

        return new LicitacionEdicion(licitacion, rowVersion);
    }

    public async Task<decimal> ObtenerMaxMontoOfertadoAsync(
        Guid licitacionId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Ofertas
            .AsNoTracking()
            .Where(o => o.LicitacionId == licitacionId)
            .MaxAsync(o => (decimal?)o.MontoOfertadoCrc, cancellationToken)
            ?? 0m;

    public async Task GuardarAsync(
        Licitacion licitacion,
        int expectedRowVersion,
        CancellationToken cancellationToken = default)
    {
        dbContext.Entry(licitacion).Property("RowVersion").OriginalValue =
            expectedRowVersion;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new LicitacionConcurrenciaException(
                "Otro usuario modificó la licitación. Recargue la página e intente de nuevo.");
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            })
        {
            throw new LicitacionDuplicadaException(
                "Ya existe una licitación registrada con ese código.");
        }
    }
}
