using Licitaciones.Application.Proveedores.Exceptions;
using Licitaciones.Application.Proveedores.Ports;
using Licitaciones.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

internal sealed class ProveedorRepository(AppDbContext dbContext)
    : IProveedorRepository
{
    private const string UniqueConstraintName =
        "ux_proveedores_nombre_normalizado";

    public Task<bool> ExisteNombreNormalizadoAsync(
        string nombreNormalizado,
        CancellationToken cancellationToken = default) =>
        dbContext.Proveedores
            .AnyAsync(
                proveedor =>
                    proveedor.NombreNormalizado == nombreNormalizado
                    && proveedor.EliminadoEn == null,
                cancellationToken);

    public async Task AgregarAsync(
        Proveedor proveedor,
        CancellationToken cancellationToken = default)
    {
        dbContext.Proveedores.Add(proveedor);

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
            throw new ProveedorDuplicadoException(
                "Ya existe un proveedor registrado con ese nombre.");
        }
    }
}
