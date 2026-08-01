using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Licitaciones.Detalle;
using Licitaciones.Application.Ofertas.Listar;
using Licitaciones.Application.Ofertas.OpcionesFiltro;
using Licitaciones.Application.Ofertas.Ports;
using Licitaciones.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

internal sealed class OfertaReadRepository(AppDbContext dbContext)
    : IOfertaReadRepository
{
    public async Task<PaginaResultado<OfertaListadoDto>> ListarAsync(
        OfertasConsulta consulta,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Oferta> ofertas = dbContext.Ofertas
            .AsNoTracking()
            .Where(oferta => oferta.Licitacion.EliminadoEn == null);

        if (consulta.LicitacionId.HasValue)
        {
            ofertas = ofertas.Where(oferta =>
                oferta.LicitacionId == consulta.LicitacionId.Value);
        }

        if (consulta.ProveedorId.HasValue)
        {
            ofertas = ofertas.Where(oferta =>
                oferta.ProveedorId == consulta.ProveedorId.Value);
        }

        ofertas = consulta.SortBy switch
        {
            OrdenOferta.MontoDescendente => ofertas
                .OrderByDescending(oferta => oferta.MontoOfertadoCrc)
                .ThenBy(oferta => oferta.FechaRegistro),
            OrdenOferta.FechaAscendente => ofertas
                .OrderBy(oferta => oferta.FechaRegistro)
                .ThenBy(oferta => oferta.MontoOfertadoCrc),
            OrdenOferta.FechaDescendente => ofertas
                .OrderByDescending(oferta => oferta.FechaRegistro)
                .ThenBy(oferta => oferta.MontoOfertadoCrc),
            _ => ofertas
                .OrderBy(oferta => oferta.MontoOfertadoCrc)
                .ThenBy(oferta => oferta.FechaRegistro)
        };

        var totalRegistros = await ofertas.CountAsync(cancellationToken);
        var elementos = await ofertas
            .Skip((consulta.Page - 1) * consulta.PageSize)
            .Take(consulta.PageSize)
            .Select(oferta => new OfertaListadoDto(
                oferta.Id,
                oferta.LicitacionId,
                oferta.Licitacion.Codigo,
                oferta.ProveedorId,
                oferta.Proveedor.Nombre,
                oferta.MontoOfertadoCrc,
                oferta.FechaRegistro))
            .ToListAsync(cancellationToken);

        return new PaginaResultado<OfertaListadoDto>(
            elementos,
            totalRegistros,
            consulta.Page,
            consulta.PageSize);
    }

    public async Task<OpcionesFiltroOfertasDto> ObtenerOpcionesFiltroAsync(
        CancellationToken cancellationToken = default)
    {
        var licitaciones = await dbContext.Licitaciones
            .AsNoTracking()
            .Where(licitacion => licitacion.EliminadoEn == null)
            .OrderBy(licitacion => licitacion.Codigo)
            .Select(licitacion => new OpcionLicitacionDto(
                licitacion.Id,
                licitacion.Codigo))
            .ToListAsync(cancellationToken);

        var proveedores = await dbContext.Proveedores
            .AsNoTracking()
            .Where(proveedor => proveedor.EliminadoEn == null)
            .OrderBy(proveedor => proveedor.Nombre)
            .Select(proveedor => new ProveedorBasicoDto(
                proveedor.Id,
                proveedor.Nombre))
            .ToListAsync(cancellationToken);

        return new OpcionesFiltroOfertasDto(licitaciones, proveedores);
    }
}
