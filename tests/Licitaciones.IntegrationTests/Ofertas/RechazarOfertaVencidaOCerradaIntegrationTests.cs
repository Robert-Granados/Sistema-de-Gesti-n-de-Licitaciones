using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Ofertas.Exceptions;
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

public sealed class RechazarOfertaVencidaOCerradaIntegrationTests : IAsyncLifetime
{
    private const string MensajeLicitacionVencida =
        "No se pueden registrar ofertas para licitaciones vencidas.";
    private const string MensajeLicitacionCerrada =
        "Solo se pueden registrar ofertas para licitaciones publicadas.";

    private static readonly DateTimeOffset T0 =
        new(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("licitaciones_hu21_test")
        .WithUsername("hu21_test_user")
        .WithPassword("hu21_test_password")
        .Build();

    private readonly MutableFakeClock _clock = new(T0);

    public Task InitializeAsync() => _postgres.StartAsync();

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task AlVencerLaLicitacion_RechazaLaOferta()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AplicarEsquemaCanonicoAsync();

        var licitacionId = await SembrarLicitacionConCierreEnUnSegundoAsync(dbContext);
        var proveedorId = await SembrarProveedorAsync(dbContext);
        var handler = scope.ServiceProvider.GetRequiredService<RegistrarOfertaHandler>();

        var antesDeVencer = await handler.HandleAsync(
            new RegistrarOfertaCommand(licitacionId, proveedorId, 500_000m));

        Assert.NotEqual(Guid.Empty, antesDeVencer.Id);

        _clock.Avanzar(TimeSpan.FromSeconds(1));

        var alVencer = await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => handler.HandleAsync(
                new RegistrarOfertaCommand(licitacionId, proveedorId, 400_000m)));

        Assert.Equal(MensajeLicitacionVencida, alVencer.Message);

        _clock.Avanzar(TimeSpan.FromSeconds(5));

        var despuesDeVencer = await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => handler.HandleAsync(
                new RegistrarOfertaCommand(licitacionId, proveedorId, 300_000m)));

        Assert.Equal(MensajeLicitacionVencida, despuesDeVencer.Message);

        dbContext.ChangeTracker.Clear();
        Assert.Equal(1, await dbContext.Ofertas.CountAsync());
    }

    [Fact]
    public async Task LicitacionCerrada_RechazaLaOferta()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AplicarEsquemaCanonicoAsync();

        var licitacionId = await SembrarLicitacionCerradaAsync(dbContext);
        var proveedorId = await SembrarProveedorAsync(dbContext);
        var handler = scope.ServiceProvider.GetRequiredService<RegistrarOfertaHandler>();

        var exception = await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => handler.HandleAsync(
                new RegistrarOfertaCommand(licitacionId, proveedorId, 100m)));

        Assert.Equal(MensajeLicitacionCerrada, exception.Message);

        dbContext.ChangeTracker.Clear();
        Assert.Equal(0, await dbContext.Ofertas.CountAsync());
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
        services.AddSingleton<IClock>(_clock);
        services.AddScoped<RegistrarOfertaHandler>();

        return services.BuildServiceProvider();
    }

    private static async Task<Guid> SembrarLicitacionConCierreEnUnSegundoAsync(
        AppDbContext dbContext)
    {
        var licitacion = new Licitacion(
            "LIC-HU21",
            "Compra de equipos",
            T0.AddSeconds(1),
            2_000_000m);
        licitacion.Publicar(T0);

        return await GuardarLicitacionAsync(dbContext, licitacion);
    }

    private static async Task<Guid> SembrarLicitacionCerradaAsync(
        AppDbContext dbContext)
    {
        var licitacion = new Licitacion(
            "LIC-HU21C",
            "Compra de mobiliario",
            T0.AddDays(10),
            1_000_000m);
        licitacion.Publicar(T0);
        licitacion.Cerrar("Cierre anticipado por adjudicación", T0);

        return await GuardarLicitacionAsync(dbContext, licitacion);
    }

    private static async Task<Guid> GuardarLicitacionAsync(
        AppDbContext dbContext,
        Licitacion licitacion)
    {
        dbContext.Add(licitacion);
        dbContext.Entry(licitacion)
            .Property<string>("CodigoNormalizado")
            .CurrentValue = licitacion.Codigo.ToUpperInvariant();
        await dbContext.SaveChangesAsync();

        return licitacion.Id;
    }

    private static async Task<Guid> SembrarProveedorAsync(AppDbContext dbContext)
    {
        var proveedor = new Proveedor("Proveedor HU21");

        dbContext.Add(proveedor);
        await dbContext.SaveChangesAsync();

        return proveedor.Id;
    }

    private sealed class MutableFakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; private set; } = now;

        public void Avanzar(TimeSpan delta) => UtcNow = UtcNow.Add(delta);
    }
}
