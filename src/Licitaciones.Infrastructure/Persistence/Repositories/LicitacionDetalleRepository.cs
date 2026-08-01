using Licitaciones.Application.Licitaciones.Ports;
using Licitaciones.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

internal sealed class LicitacionDetalleRepository(AppDbContext dbContext)
    : ILicitacionDetalleRepository
{
    public async Task<LicitacionDetalleCompleta?> ObtenerPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var licitacion = await dbContext.Licitaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (licitacion is null)
        {
            return null;
        }

        var ofertasRaw = await dbContext.Ofertas
            .AsNoTracking()
            .Where(o => o.LicitacionId == id)
            .Join(
                dbContext.Proveedores.Where(p => p.EliminadoEn == null),
                oferta => oferta.ProveedorId,
                proveedor => proveedor.Id,
                (oferta, proveedor) => new
                {
                    oferta.Id,
                    ProveedorId = proveedor.Id,
                    proveedor.Nombre,
                    oferta.MontoOfertadoCrc,
                    oferta.FechaRegistro
                })
            .OrderBy(o => o.MontoOfertadoCrc)
            .ThenBy(o => o.FechaRegistro)
            .ToListAsync(cancellationToken);

        var ofertas = ofertasRaw
            .Select(o => new OfertaBasica(
                o.Id,
                o.ProveedorId,
                o.Nombre,
                o.MontoOfertadoCrc,
                o.FechaRegistro))
            .ToList();

        var niveles = await dbContext.NivelesAprobacion
            .AsNoTracking()
            .OrderBy(n => n.MontoMinimoCrc)
            .ToListAsync(cancellationToken);

        var tipoCambioActivo = await dbContext.TiposCambio
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Activo, cancellationToken);

        var proveedoresDisponibles = await dbContext.Proveedores
            .AsNoTracking()
            .Where(p => p.EliminadoEn == null)
            .OrderBy(p => p.Nombre)
            .Select(p => new ProveedorBasico(p.Id, p.Nombre))
            .ToListAsync(cancellationToken);

        return new LicitacionDetalleCompleta(
            licitacion,
            ofertas,
            niveles,
            tipoCambioActivo,
            proveedoresDisponibles);
    }
}
