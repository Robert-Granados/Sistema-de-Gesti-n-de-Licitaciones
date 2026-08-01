using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Licitaciones.Cerrar;
using Licitaciones.Application.Ofertas.Common;
using Licitaciones.Application.Ofertas.Editar;
using Licitaciones.Application.Ofertas.Eliminar;
using Licitaciones.Application.Ofertas.Exceptions;
using Licitaciones.Application.Ofertas.Ports;
using Licitaciones.Application.Ofertas.Registrar;
using Licitaciones.Domain.Entities;
using Licitaciones.Infrastructure;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Licitaciones.IntegrationTests.Ofertas;

public sealed class EditarEliminarOfertaIntegrationTests : IAsyncLifetime
{
    private const string MensajeLicitacionCerrada = "No se pueden modificar ni eliminar ofertas de licitaciones cerradas.";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("licitaciones_hu2324_test")
        .WithUsername("hu2324_test_user")
        .WithPassword("hu2324_test_password")
        .Build();

    public Task InitializeAsync() => _postgres.StartAsync();

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task Editar_LicitacionAbierta_ActualizaElMonto()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AplicarEsquemaCanonicoAsync();

        var (datos, _) = await SembrarOfertaEnLicitacionAbiertaAsync(dbContext, scope);
        var handler = scope.ServiceProvider.GetRequiredService<EditarOfertaHandler>();

        await handler.HandleAsync(new EditarOfertaCommand(datos.OfertaId, 600_000m));

        dbContext.ChangeTracker.Clear();
        var oferta = await dbContext.Ofertas
            .SingleAsync(o => o.Id == datos.OfertaId);
        Assert.Equal(600_000m, oferta.MontoOfertadoCrc);
    }

    [Fact]
    public async Task Editar_LicitacionVencida_RechazaSinActualizar()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AplicarEsquemaCanonicoAsync();

        var (datos, clock) = await SembrarOfertaEnLicitacionAbiertaAsync(dbContext, scope);
        var handler = scope.ServiceProvider.GetRequiredService<EditarOfertaHandler>();

        clock.Avanzar(TimeSpan.FromDays(2));

        var exception = await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => handler.HandleAsync(new EditarOfertaCommand(datos.OfertaId, 600_000m)));

        Assert.Equal(
            "No se pueden editar ofertas para licitaciones vencidas.",
            exception.Message);

        dbContext.ChangeTracker.Clear();
        var oferta = await dbContext.Ofertas
            .SingleAsync(o => o.Id == datos.OfertaId);
        Assert.Equal(500_000m, oferta.MontoOfertadoCrc);
    }

    [Fact]
    public async Task Editar_LicitacionCerrada_RechazaSinActualizar()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AplicarEsquemaCanonicoAsync();

        var (datos, _) = await SembrarOfertaEnLicitacionAbiertaAsync(dbContext, scope);
        await scope.ServiceProvider
            .GetRequiredService<CerrarLicitacionHandler>()
            .HandleAsync(new CerrarLicitacionCommand(datos.LicitacionId, "Cierre de prueba"));
        var handler = scope.ServiceProvider.GetRequiredService<EditarOfertaHandler>();

        var exception = await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => handler.HandleAsync(new EditarOfertaCommand(datos.OfertaId, 600_000m)));

        Assert.Equal(
            "Solo se pueden editar ofertas para licitaciones publicadas.",
            exception.Message);

        dbContext.ChangeTracker.Clear();
        var oferta = await dbContext.Ofertas
            .SingleAsync(o => o.Id == datos.OfertaId);
        Assert.Equal(500_000m, oferta.MontoOfertadoCrc);
    }

    [Fact]
    public async Task Eliminar_LicitacionAbierta_EliminaLaOferta()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AplicarEsquemaCanonicoAsync();

        var (datos, _) = await SembrarOfertaEnLicitacionAbiertaAsync(dbContext, scope);
        var handler = scope.ServiceProvider.GetRequiredService<EliminarOfertaHandler>();

        var result = await handler.HandleAsync(new EliminarOfertaCommand(datos.OfertaId));

        Assert.Equal(datos.OfertaId, result.Id);
        dbContext.ChangeTracker.Clear();
        Assert.Equal(0, await dbContext.Ofertas.CountAsync());
    }

    [Fact]
    public async Task Eliminar_LicitacionVencida_RechazaSinEliminar()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AplicarEsquemaCanonicoAsync();

        var (datos, clock) = await SembrarOfertaEnLicitacionAbiertaAsync(dbContext, scope);
        var handler = scope.ServiceProvider.GetRequiredService<EliminarOfertaHandler>();

        clock.Avanzar(TimeSpan.FromDays(2));

        var exception = await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => handler.HandleAsync(new EliminarOfertaCommand(datos.OfertaId)));

        Assert.Equal(
            "No se pueden eliminar ofertas para licitaciones vencidas.",
            exception.Message);

        dbContext.ChangeTracker.Clear();
        Assert.Equal(1, await dbContext.Ofertas.CountAsync());
    }

    [Fact]
    public async Task Eliminar_LicitacionCerrada_RechazaSinEliminar()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AplicarEsquemaCanonicoAsync();

        var (datos, _) = await SembrarOfertaEnLicitacionAbiertaAsync(dbContext, scope);
        await scope.ServiceProvider
            .GetRequiredService<CerrarLicitacionHandler>()
            .HandleAsync(new CerrarLicitacionCommand(datos.LicitacionId, "Cierre de prueba"));
        var handler = scope.ServiceProvider.GetRequiredService<EliminarOfertaHandler>();

        var exception = await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => handler.HandleAsync(new EliminarOfertaCommand(datos.OfertaId)));

        Assert.Equal(
            "Solo se pueden eliminar ofertas para licitaciones publicadas.",
            exception.Message);

        dbContext.ChangeTracker.Clear();
        Assert.Equal(1, await dbContext.Ofertas.CountAsync());
    }

    [Fact]
    public async Task Eliminar_LicitacionCerrada_CapaDeDatosTambienLaBloquea()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AplicarEsquemaCanonicoAsync();

        var (datos, _) = await SembrarOfertaEnLicitacionAbiertaAsync(dbContext, scope);
        await scope.ServiceProvider
            .GetRequiredService<CerrarLicitacionHandler>()
            .HandleAsync(new CerrarLicitacionCommand(datos.LicitacionId, "Cierre de prueba"));

        var writeRepository = scope.ServiceProvider
            .GetRequiredService<IOfertaWriteRepository>();
        var oferta = await writeRepository.ObtenerPorIdAsync(datos.OfertaId);

        Assert.NotNull(oferta);

        var exception = await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => writeRepository.EliminarAsync(oferta!));

        Assert.Equal(MensajeLicitacionCerrada, exception.Message);

        dbContext.ChangeTracker.Clear();
        Assert.Equal(1, await dbContext.Ofertas.CountAsync());
    }

    private async Task<(DatosSembrados Datos, MutableFakeClock Clock)>
        SembrarOfertaEnLicitacionAbiertaAsync(
            AppDbContext dbContext,
            IServiceScope scope)
    {
        var ahora = DateTimeOffset.UtcNow;
        var licitacion = new Licitacion(
            "LIC-HU2324",
            "Compra de equipos de cómputo",
            ahora.AddDays(1),
            1_000_000m);
        licitacion.Publicar(ahora);
        var proveedor = new Proveedor("Proveedor HU2324");

        dbContext.AddRange(licitacion, proveedor);
        dbContext.Entry(licitacion)
            .Property<string>("CodigoNormalizado")
            .CurrentValue = licitacion.Codigo.ToUpperInvariant();
        await dbContext.SaveChangesAsync();

        var clock = scope.ServiceProvider.GetRequiredService<IClock>() as MutableFakeClock
            ?? throw new InvalidOperationException("Se esperaba un reloj de prueba.");
        var registrarHandler = scope.ServiceProvider
            .GetRequiredService<RegistrarOfertaHandler>();
        var result = await registrarHandler.HandleAsync(
            new RegistrarOfertaCommand(
                licitacion.Id,
                proveedor.Id,
                500_000m));

        return (
            new DatosSembrados(licitacion.Id, result.Id),
            clock);
    }

    private async Task AplicarEsquemaCanonicoAsync()
    {
        var rutaEsquema = Path.Combine(
            AppContext.BaseDirectory, "database_schema.sql");
        var esquema = await File.ReadAllTextAsync(rutaEsquema);

        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(esquema, connection);
        await command.ExecuteNonQueryAsync();
    }

    private ServiceProvider CrearServicios()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        services.AddSingleton<IClock>(new MutableFakeClock(DateTimeOffset.UtcNow));
        services.AddScoped<OfertaValidador>();
        services.AddScoped<RegistrarOfertaHandler>();
        services.AddScoped<EditarOfertaHandler>();
        services.AddScoped<EliminarOfertaHandler>();
        services.AddScoped<CerrarLicitacionHandler>();

        return services.BuildServiceProvider();
    }

    private sealed record DatosSembrados(
        Guid LicitacionId,
        Guid OfertaId);

    private sealed class MutableFakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; private set; } = now;

        public void Avanzar(TimeSpan delta) => UtcNow = UtcNow.Add(delta);
    }
}
