using Licitaciones.Application.Proveedores.Exceptions;
using Licitaciones.Application.Proveedores.Ports;
using Licitaciones.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

internal sealed class ProveedorEditRepository(AppDbContext dbContext)
    : IProveedorEditRepository
{
    private const string UniqueConstraintName =
        "ux_proveedores_nombre_normalizado";

    public async Task<ProveedorEdicion?> ObtenerPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var proveedor = await dbContext.Proveedores
            .SingleOrDefaultAsync(
                item => item.Id == id && item.EliminadoEn == null,
                cancellationToken);

        return proveedor is null
            ? null
            : new ProveedorEdicion(
                proveedor,
                dbContext.Entry(proveedor).Property<int>("RowVersion").CurrentValue);
    }

    public Task<bool> ExisteNombreNormalizadoAsync(
        string nombreNormalizado,
        Guid excluirProveedorId,
        CancellationToken cancellationToken = default) =>
        dbContext.Proveedores.AnyAsync(
            proveedor =>
                proveedor.Id != excluirProveedorId
                && proveedor.EliminadoEn == null
                && proveedor.NombreNormalizado == nombreNormalizado,
            cancellationToken);

    public async Task GuardarAsync(
        Proveedor proveedor,
        int expectedRowVersion,
        CancellationToken cancellationToken = default)
    {
        dbContext.Entry(proveedor)
            .Property<int>("RowVersion")
            .OriginalValue = expectedRowVersion;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProveedorConcurrenciaException(
                "El registro fue modificado por otro usuario.");
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: UniqueConstraintName
            })
        {
            throw new ProveedorDuplicadoException(
                "Ya existe otro proveedor registrado con ese nombre.");
        }
    }
}

