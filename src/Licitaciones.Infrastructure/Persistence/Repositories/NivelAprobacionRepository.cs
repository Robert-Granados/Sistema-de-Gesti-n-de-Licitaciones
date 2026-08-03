using Licitaciones.Application.NivelesAprobacion;
using Licitaciones.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

internal sealed class NivelAprobacionRepository(AppDbContext dbContext)
    : INivelAprobacionRepository
{
    public async Task<IReadOnlyList<NivelAprobacion>> ListarOrdenadosAsync(
        CancellationToken cancellationToken = default) =>
        await dbContext.NivelesAprobacion
            .OrderBy(n => n.MontoMinimoCrc)
            .ToListAsync(cancellationToken);

    public Task<NivelAprobacion?> ObtenerAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.NivelesAprobacion.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task AgregarAsync(
        NivelAprobacion nivel,
        CancellationToken cancellationToken = default)
    {
        dbContext.NivelesAprobacion.Add(nivel);
        await GuardarConRestriccionesAsync(cancellationToken);
    }

    public Task GuardarAsync(CancellationToken cancellationToken = default) =>
        GuardarConRestriccionesAsync(cancellationToken);

    public async Task EliminarAsync(
        NivelAprobacion nivel,
        CancellationToken cancellationToken = default)
    {
        dbContext.NivelesAprobacion.Remove(nivel);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task GuardarConRestriccionesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException postgres
                && postgres.ConstraintName is
                    "ex_niveles_rango_sin_traslape"
                    or "ux_niveles_aprobacion_unico_abierto")
        {
            throw new NivelAprobacionException(
                "El rango se traslapa con otro nivel o ya existe un rango abierto.");
        }
    }
}
