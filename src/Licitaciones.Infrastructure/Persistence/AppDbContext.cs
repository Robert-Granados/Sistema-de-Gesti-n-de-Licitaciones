using Licitaciones.Application.Common.Clock;
using Licitaciones.Domain.Entities;
using Licitaciones.Domain.Enums;
using Licitaciones.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    private readonly IClock _clock;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IClock clock) : base(options)
    {
        _clock = clock;
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : this(options, new SystemClock())
    {
    }

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
        AplicarAuditoria();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        await ValidarProveedoresActivosEnOfertasAsync(cancellationToken);
        AplicarAuditoria();
        return await base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken);
    }

    private void AplicarAuditoria()
    {
        ChangeTracker.DetectChanges();
        var now = _clock.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            var createdAt = entry.Metadata.FindProperty("CreatedAt");
            var updatedAt = entry.Metadata.FindProperty("UpdatedAt");
            var deletedAt = entry.Metadata.FindProperty("DeletedAt")
                ?? entry.Metadata.FindProperty("EliminadoEn");

            if (entry.State == EntityState.Deleted && deletedAt is not null)
            {
                entry.State = EntityState.Modified;
                entry.Property(deletedAt.Name).CurrentValue = now;
            }

            if (entry.State == EntityState.Added)
            {
                if (createdAt is not null)
                {
                    entry.Property(createdAt.Name).CurrentValue = now;
                }

                if (updatedAt is not null)
                {
                    entry.Property(updatedAt.Name).CurrentValue = now;
                }

                continue;
            }

            if (entry.State != EntityState.Modified)
            {
                continue;
            }

            if (createdAt is not null)
            {
                var property = entry.Property(createdAt.Name);
                property.CurrentValue = property.OriginalValue;
                property.IsModified = false;
            }

            if (updatedAt is not null)
            {
                var property = entry.Property(updatedAt.Name);
                property.CurrentValue = now;
                property.IsModified = true;
            }

            if (deletedAt is not null)
            {
                var property = entry.Property(deletedAt.Name);
                if (property.IsModified
                    && property.OriginalValue is null
                    && property.CurrentValue is not null)
                {
                    property.CurrentValue = now;
                }
            }
        }
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
