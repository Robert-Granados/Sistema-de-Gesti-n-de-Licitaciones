using Licitaciones.Application.Proveedores.Ports;
using Licitaciones.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

internal sealed class ProveedorDeleteRepository(AppDbContext dbContext)
    : IProveedorDeleteRepository
{
    public Task<Proveedor?> ObtenerActivoPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.Proveedores.SingleOrDefaultAsync(
            proveedor => proveedor.Id == id
                && proveedor.EliminadoEn == null,
            cancellationToken);

    public Task<bool> TieneOfertasAsync(
        Guid proveedorId,
        CancellationToken cancellationToken = default) =>
        dbContext.Ofertas.AnyAsync(
            oferta => oferta.ProveedorId == proveedorId,
            cancellationToken);

    public async Task GuardarBorradoLogicoAsync(
        Proveedor proveedor,
        CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
