using Licitaciones.Application.Licitaciones.Ports;
using Licitaciones.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

internal sealed class LicitacionDeleteRepository(AppDbContext dbContext)
    : ILicitacionDeleteRepository
{
    public Task<Licitacion?> ObtenerActivaPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.Licitaciones.SingleOrDefaultAsync(
            licitacion => licitacion.Id == id
                && licitacion.EliminadoEn == null,
            cancellationToken);

    public Task<bool> TieneOfertasAsync(
        Guid licitacionId,
        CancellationToken cancellationToken = default) =>
        dbContext.Ofertas.AnyAsync(
            oferta => oferta.LicitacionId == licitacionId,
            cancellationToken);

    public async Task GuardarBorradoLogicoAsync(
        Licitacion licitacion,
        CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
