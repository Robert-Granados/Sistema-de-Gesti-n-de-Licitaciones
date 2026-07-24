using Licitaciones.Application.Proveedores.Eliminar;
using Licitaciones.Application.Proveedores.Ports;
using Licitaciones.Domain.Entities;
using Licitaciones.Infrastructure.Persistence;
using Licitaciones.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Licitaciones.IntegrationTests.Proveedores;

public sealed class EliminarProveedorIntegrationTests
{
    [Fact]
    public async Task EliminarProveedorConOferta_ConservaProveedorEHistorial()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var proveedor = new Proveedor("Proveedor con historial", "PROVEEDOR CON HISTORIAL");
        var licitacion = new Licitacion(
            "LIC-001",
            "Compra de suministros",
            DateTimeOffset.UtcNow.AddDays(10),
            500_000m);
        var oferta = new Oferta(
            licitacion.Id,
            proveedor.Id,
            125_000m,
            DateTimeOffset.UtcNow);

        dbContext.AddRange(proveedor, licitacion, oferta);
        dbContext.Entry(licitacion)
            .Property<string>("CodigoNormalizado")
            .CurrentValue = licitacion.Codigo.ToUpperInvariant();
        await dbContext.SaveChangesAsync();

        var handler = scope.ServiceProvider
            .GetRequiredService<EliminarProveedorHandler>();
        var resultado = await handler.HandleAsync(
            new EliminarProveedorCommand(proveedor.Id));

        dbContext.ChangeTracker.Clear();
        var proveedorPersistido = await dbContext.Proveedores
            .SingleAsync(item => item.Id == proveedor.Id);
        var ofertaPersistida = await dbContext.Ofertas
            .SingleAsync(item => item.Id == oferta.Id);

        Assert.True(resultado.TeniaOfertas);
        Assert.NotNull(proveedorPersistido.EliminadoEn);
        Assert.Equal(proveedor.Id, ofertaPersistida.ProveedorId);
    }

    [Fact]
    public async Task CrearOfertaParaProveedorEliminado_EsRechazado()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var proveedor = new Proveedor("Proveedor eliminado", "PROVEEDOR ELIMINADO");
        var licitacion = new Licitacion(
            "LIC-002",
            "Compra de equipo",
            DateTimeOffset.UtcNow.AddDays(10),
            500_000m);

        dbContext.AddRange(proveedor, licitacion);
        dbContext.Entry(licitacion)
            .Property<string>("CodigoNormalizado")
            .CurrentValue = licitacion.Codigo.ToUpperInvariant();
        await dbContext.SaveChangesAsync();
        proveedor.Eliminar(DateTimeOffset.UtcNow);
        await dbContext.SaveChangesAsync();

        dbContext.Ofertas.Add(new Oferta(
            licitacion.Id,
            proveedor.Id,
            250_000m,
            DateTimeOffset.UtcNow));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => dbContext.SaveChangesAsync());

        Assert.Equal(
            "No se pueden registrar ofertas para un proveedor eliminado.",
            exception.Message);
    }

    private static ServiceProvider CrearServicios()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddScoped<IProveedorDeleteRepository, ProveedorDeleteRepository>();
        services.AddScoped<EliminarProveedorHandler>();

        return services.BuildServiceProvider();
    }
}
