using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Proveedores.Listar;
using Licitaciones.Application.Proveedores.Ports;
using Licitaciones.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

internal sealed class ProveedorReadRepository(AppDbContext dbContext)
    : IProveedorReadRepository
{
    public async Task<PaginaResultado<ProveedorListadoDto>> ListarAsync(
        ProveedoresConsulta consulta,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Proveedor> proveedores = dbContext.Proveedores
            .AsNoTracking()
            .Where(proveedor => proveedor.EliminadoEn == null);

        if (!string.IsNullOrWhiteSpace(consulta.Search))
        {
            proveedores = proveedores.Where(proveedor =>
                proveedor.NombreNormalizado.Contains(consulta.Search));
        }

        proveedores = consulta.SortBy switch
        {
            OrdenProveedor.NombreDescendente => proveedores
                .OrderByDescending(proveedor => proveedor.NombreNormalizado)
                .ThenBy(proveedor => proveedor.Id),
            OrdenProveedor.IdAscendente => proveedores
                .OrderBy(proveedor => proveedor.Id),
            OrdenProveedor.IdDescendente => proveedores
                .OrderByDescending(proveedor => proveedor.Id),
            _ => proveedores
                .OrderBy(proveedor => proveedor.NombreNormalizado)
                .ThenBy(proveedor => proveedor.Id)
        };

        var totalRegistros = await proveedores.CountAsync(cancellationToken);
        var elementos = await proveedores
            .Skip((consulta.Page - 1) * consulta.PageSize)
            .Take(consulta.PageSize)
            .Select(proveedor =>
                new ProveedorListadoDto(proveedor.Id, proveedor.Nombre))
            .ToListAsync(cancellationToken);

        return new PaginaResultado<ProveedorListadoDto>(
            elementos,
            totalRegistros,
            consulta.Page,
            consulta.PageSize);
    }
}

