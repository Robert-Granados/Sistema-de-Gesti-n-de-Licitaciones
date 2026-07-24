using Licitaciones.Domain.Entities;
using Licitaciones.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Licitacion> Licitaciones => Set<Licitacion>();

    public DbSet<Proveedor> Proveedores => Set<Proveedor>();

    public DbSet<Oferta> Ofertas => Set<Oferta>();

    public DbSet<NivelAprobacion> NivelesAprobacion => Set<NivelAprobacion>();

    public DbSet<TipoCambio> TiposCambio => Set<TipoCambio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("unaccent");
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidarProveedoresActivosEnOfertas();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        await ValidarProveedoresActivosEnOfertasAsync(cancellationToken);
        return await base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken);
    }

    private void ValidarProveedoresActivosEnOfertas()
    {
        var proveedorIds = ObtenerProveedorIdsDeOfertasNuevas();
        var proveedorIdsPendientes = ObtenerProveedorIdsPendientes(proveedorIds);
        if (proveedorIdsPendientes.Count == 0)
        {
            return;
        }

        var proveedoresValidos = Proveedores.Count(
            proveedor => proveedorIdsPendientes.Contains(proveedor.Id)
                && proveedor.EliminadoEn == null);

        if (proveedoresValidos != proveedorIdsPendientes.Count)
        {
            throw new InvalidOperationException(
                "No se pueden registrar ofertas para un proveedor eliminado.");
        }
    }

    private async Task ValidarProveedoresActivosEnOfertasAsync(
        CancellationToken cancellationToken)
    {
        var proveedorIds = ObtenerProveedorIdsDeOfertasNuevas();
        var proveedorIdsPendientes = ObtenerProveedorIdsPendientes(proveedorIds);
        if (proveedorIdsPendientes.Count == 0)
        {
            return;
        }

        var proveedoresValidos = await Proveedores.CountAsync(
            proveedor => proveedorIdsPendientes.Contains(proveedor.Id)
                && proveedor.EliminadoEn == null,
            cancellationToken);

        if (proveedoresValidos != proveedorIdsPendientes.Count)
        {
            throw new InvalidOperationException(
                "No se pueden registrar ofertas para un proveedor eliminado.");
        }
    }

    private IReadOnlySet<Guid> ObtenerProveedorIdsDeOfertasNuevas() =>
        ChangeTracker.Entries<Oferta>()
            .Where(entry => entry.State == EntityState.Added)
            .Select(entry => entry.Entity.ProveedorId)
            .ToHashSet();

    private IReadOnlySet<Guid> ObtenerProveedorIdsPendientes(
        IReadOnlySet<Guid> proveedorIds)
    {
        var proveedoresActivosAgregados = ChangeTracker.Entries<Proveedor>()
            .Where(entry => entry.State == EntityState.Added
                && entry.Entity.EliminadoEn == null)
            .Select(entry => entry.Entity.Id)
            .ToHashSet();

        return proveedorIds
            .Where(id => !proveedoresActivosAgregados.Contains(id))
            .ToHashSet();
    }
}
