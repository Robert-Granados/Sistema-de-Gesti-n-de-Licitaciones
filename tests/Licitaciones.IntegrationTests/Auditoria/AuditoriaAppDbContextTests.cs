using Licitaciones.Application.Common.Clock;
using Licitaciones.Domain.Entities;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.IntegrationTests.Auditoria;

public sealed class AuditoriaAppDbContextTests
{
    [Fact]
    public async Task Guardar_CreacionYModificaciones_AdministraFechasConIClock()
    {
        var createdAt = new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);
        var updatedAt = createdAt.AddHours(2);
        var clock = new FakeClock(createdAt);
        await using var context = CrearContexto(clock);
        var proveedor = new Proveedor("Proveedor auditado", "PROVEEDOR AUDITADO");

        context.Proveedores.Add(proveedor);
        await context.SaveChangesAsync();

        var entry = context.Entry(proveedor);
        Assert.Equal(createdAt, entry.Property<DateTimeOffset>("CreatedAt").CurrentValue);
        Assert.Equal(createdAt, entry.Property<DateTimeOffset>("UpdatedAt").CurrentValue);

        clock.UtcNow = updatedAt;
        proveedor.CambiarNombre("Proveedor actualizado", "PROVEEDOR ACTUALIZADO");
        entry.Property<DateTimeOffset>("CreatedAt").CurrentValue = createdAt.AddYears(-5);
        await context.SaveChangesAsync();

        Assert.Equal(createdAt, entry.Property<DateTimeOffset>("CreatedAt").CurrentValue);
        Assert.Equal(updatedAt, entry.Property<DateTimeOffset>("UpdatedAt").CurrentValue);
    }

    [Fact]
    public async Task Guardar_BorradoLogico_ReemplazaFechaDelLlamadorConIClock()
    {
        var createdAt = new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);
        var deletedAt = createdAt.AddDays(1);
        var clock = new FakeClock(createdAt);
        await using var context = CrearContexto(clock);
        var proveedor = new Proveedor("Proveedor por eliminar", "PROVEEDOR POR ELIMINAR");
        context.Proveedores.Add(proveedor);
        await context.SaveChangesAsync();

        clock.UtcNow = deletedAt;
        proveedor.Eliminar(DateTimeOffset.UnixEpoch);
        await context.SaveChangesAsync();

        Assert.Equal(deletedAt, proveedor.EliminadoEn);
        Assert.Equal(
            deletedAt,
            context.Entry(proveedor).Property<DateTimeOffset>("UpdatedAt").CurrentValue);
    }

    [Fact]
    public async Task Remove_EntidadConBorradoLogico_NoLaEliminaFisicamente()
    {
        var now = new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);
        var clock = new FakeClock(now);
        await using var context = CrearContexto(clock);
        var proveedor = new Proveedor("Proveedor removido", "PROVEEDOR REMOVIDO");
        context.Proveedores.Add(proveedor);
        await context.SaveChangesAsync();

        context.Proveedores.Remove(proveedor);
        await context.SaveChangesAsync();

        Assert.Equal(EntityState.Unchanged, context.Entry(proveedor).State);
        Assert.Equal(now, proveedor.EliminadoEn);
        Assert.Contains(proveedor, context.Proveedores.Local);
    }

    private static AppDbContext CrearContexto(IClock clock)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options, clock);
    }

    private sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = utcNow;
    }
}
