using Licitaciones.Application.Licitaciones.Exceptions;
using Licitaciones.Application.Licitaciones.Ports;
using Licitaciones.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

internal sealed class LicitacionRepository(AppDbContext dbContext)
    : ILicitacionRepository
{
    private const string UniqueConstraintName =
        "ux_licitaciones_codigo_normalizado";

    public Task<bool> ExisteCodigoNormalizadoAsync(
        string codigoNormalizado,
        CancellationToken cancellationToken = default) =>
        dbContext.Licitaciones
            .AnyAsync(
                licitacion =>
                    EF.Property<string>(licitacion, "CodigoNormalizado") == codigoNormalizado,
                cancellationToken);

    public async Task AgregarAsync(
        Licitacion licitacion,
        CancellationToken cancellationToken = default)
    {
        dbContext.Licitaciones.Add(licitacion);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: UniqueConstraintName
            })
        {
            throw new LicitacionDuplicadaException(
                "Ya existe una licitación registrada con ese código.");
        }
    }
}
