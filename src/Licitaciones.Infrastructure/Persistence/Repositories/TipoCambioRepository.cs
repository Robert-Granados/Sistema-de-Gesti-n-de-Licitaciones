using Licitaciones.Application.TiposCambio;
using Licitaciones.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

internal sealed class TipoCambioRepository(AppDbContext dbContext)
    : ITipoCambioRepository
{
    public async Task<IReadOnlyList<TipoCambio>> ListarAsync(
        CancellationToken cancellationToken = default) =>
        await dbContext.TiposCambio
            .AsNoTracking()
            .OrderByDescending(t => t.FechaVigencia)
            .ToListAsync(cancellationToken);

    public Task<TipoCambio?> ObtenerAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.TiposCambio.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task AgregarAsync(
        TipoCambio tipoCambio,
        CancellationToken cancellationToken = default)
    {
        dbContext.TiposCambio.Add(tipoCambio);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task GuardarAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);

    public async Task EliminarAsync(
        TipoCambio tipoCambio,
        CancellationToken cancellationToken = default)
    {
        dbContext.TiposCambio.Remove(tipoCambio);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ActivarEnTransaccionAsync(
        TipoCambio tipoCambio,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await dbContext.TiposCambio
            .Where(t => t.Activo && t.Id != tipoCambio.Id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(t => t.Activo, false),
                cancellationToken);

        tipoCambio.Activar();
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
