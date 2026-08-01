using Licitaciones.Application.Licitaciones.Exceptions;
using Licitaciones.Application.Ofertas.Exceptions;
using Licitaciones.Application.Ofertas.Ports;
using Licitaciones.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

internal sealed class OfertaRepository(AppDbContext dbContext)
    : IOfertaValidacionRepository, IOfertaWriteRepository
{
    private const string UniqueConstraintName =
        "ux_ofertas_licitacion_proveedor";

    public Task<bool> ExisteLicitacionPublicadaAsync(
        Guid licitacionId,
        CancellationToken cancellationToken = default) =>
        dbContext.Licitaciones.AnyAsync(
            l => l.Id == licitacionId
                && l.Estado == Domain.Enums.EstadoLicitacion.Publicada,
            cancellationToken);

    public Task<DateTimeOffset?> ObtenerFechaCierreAsync(
        Guid licitacionId,
        CancellationToken cancellationToken = default) =>
        dbContext.Licitaciones
            .Where(l => l.Id == licitacionId)
            .Select(l => (DateTimeOffset?)l.FechaCierre)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<decimal> ObtenerPresupuestoAsync(
        Guid licitacionId,
        CancellationToken cancellationToken = default) =>
        dbContext.Licitaciones
            .Where(l => l.Id == licitacionId)
            .Select(l => l.PresupuestoEstimadoCrc)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> ProveedorExisteAsync(
        Guid proveedorId,
        CancellationToken cancellationToken = default) =>
        dbContext.Proveedores.AnyAsync(
            p => p.Id == proveedorId && p.EliminadoEn == null,
            cancellationToken);

    public Task<bool> YaTieneOfertaAsync(
        Guid licitacionId,
        Guid proveedorId,
        CancellationToken cancellationToken = default) =>
        dbContext.Ofertas.AnyAsync(
            o => o.LicitacionId == licitacionId
                && o.ProveedorId == proveedorId,
            cancellationToken);

    public async Task AgregarAsync(
        Oferta oferta,
        CancellationToken cancellationToken = default)
    {
        dbContext.Ofertas.Add(oferta);

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
            throw new OfertaDuplicadaException(
                "Este proveedor ya tiene una oferta registrada para esta licitación.");
        }
    }
}
