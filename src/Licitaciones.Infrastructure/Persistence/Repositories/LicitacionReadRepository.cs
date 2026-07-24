using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Licitaciones.Listar;
using Licitaciones.Application.Licitaciones.Ports;
using Licitaciones.Domain.Entities;
using Licitaciones.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

internal sealed class LicitacionReadRepository(AppDbContext dbContext)
    : ILicitacionReadRepository
{
    public async Task<PaginaResultado<LicitacionListadoDto>> ListarAsync(
        LicitacionesConsulta consulta,
        DateTimeOffset ahoraUtc,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Licitacion> licitaciones = dbContext.Licitaciones
            .AsNoTracking()
            .Where(l => l.Estado != EstadoLicitacion.Cerrada);

        if (!string.IsNullOrWhiteSpace(consulta.Search))
        {
            licitaciones = licitaciones.Where(l =>
                l.Codigo.Contains(consulta.Search)
                || l.Titulo.Contains(consulta.Search));
        }

        if (!string.IsNullOrWhiteSpace(consulta.FiltroEstado)
            && Enum.TryParse<EstadoLicitacion>(consulta.FiltroEstado, true, out var estado))
        {
            licitaciones = licitaciones.Where(l => l.Estado == estado);
        }

        if (consulta.FechaDesde.HasValue)
        {
            licitaciones = licitaciones.Where(l =>
                l.FechaCierre >= consulta.FechaDesde.Value);
        }

        if (consulta.FechaHasta.HasValue)
        {
            licitaciones = licitaciones.Where(l =>
                l.FechaCierre <= consulta.FechaHasta.Value);
        }

        licitaciones = consulta.SortBy switch
        {
            OrdenLicitacion.FechaCierreDescendente => licitaciones
                .OrderByDescending(l => l.FechaCierre)
                .ThenBy(l => l.Codigo),
            OrdenLicitacion.CodigoAscendente => licitaciones
                .OrderBy(l => l.Codigo)
                .ThenBy(l => l.FechaCierre),
            OrdenLicitacion.CodigoDescendente => licitaciones
                .OrderByDescending(l => l.Codigo)
                .ThenBy(l => l.FechaCierre),
            _ => licitaciones
                .OrderBy(l => l.FechaCierre)
                .ThenBy(l => l.Codigo)
        };

        var totalRegistros = await licitaciones.CountAsync(cancellationToken);
        var elementos = await licitaciones
            .Skip((consulta.Page - 1) * consulta.PageSize)
            .Take(consulta.PageSize)
            .Select(l => new LicitacionListadoDto(
                l.Id,
                l.Codigo,
                l.Titulo,
                l.Estado,
                l.FechaCierre,
                l.PresupuestoEstimadoCrc,
                l.Estado == EstadoLicitacion.Publicada
                    && l.FechaCierre <= ahoraUtc))
            .ToListAsync(cancellationToken);

        return new PaginaResultado<LicitacionListadoDto>(
            elementos,
            totalRegistros,
            consulta.Page,
            consulta.PageSize);
    }
}
