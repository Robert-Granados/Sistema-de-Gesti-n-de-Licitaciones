using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Proveedores.Detalle;
using Licitaciones.Application.Proveedores.Ports;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

internal sealed class ProveedorDetalleRepository(AppDbContext dbContext)
    : IProveedorDetalleRepository
{
    public async Task<ProveedorDetalleDto?> ObtenerPorIdAsync(
        ProveedorDetalleConsulta consulta,
        CancellationToken cancellationToken = default)
    {
        var proveedor = await dbContext.Proveedores
            .AsNoTracking()
            .Where(item =>
                item.Id == consulta.Id
                && item.EliminadoEn == null)
            .Include(item => item.Ofertas
                .OrderByDescending(oferta => oferta.FechaRegistro)
                .ThenBy(oferta => oferta.Id)
                .Skip((consulta.Page - 1) * consulta.PageSize)
                .Take(consulta.PageSize))
            .ThenInclude(oferta => oferta.Licitacion)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);

        if (proveedor is null)
        {
            return null;
        }

        var totalOfertas = await dbContext.Ofertas
            .AsNoTracking()
            .CountAsync(
                oferta => oferta.ProveedorId == consulta.Id,
                cancellationToken);

        var ofertas = proveedor.Ofertas
            .OrderByDescending(oferta => oferta.FechaRegistro)
            .ThenBy(oferta => oferta.Id)
            .Select(oferta => new OfertaProveedorDto(
                oferta.Id,
                oferta.LicitacionId,
                oferta.Licitacion.Codigo,
                oferta.Licitacion.Titulo,
                oferta.MontoOfertadoCrc,
                oferta.FechaRegistro,
                oferta.Licitacion.Estado))
            .ToList();

        return new ProveedorDetalleDto(
            proveedor.Id,
            proveedor.Nombre,
            new PaginaResultado<OfertaProveedorDto>(
                ofertas,
                totalOfertas,
                consulta.Page,
                consulta.PageSize));
    }
}
