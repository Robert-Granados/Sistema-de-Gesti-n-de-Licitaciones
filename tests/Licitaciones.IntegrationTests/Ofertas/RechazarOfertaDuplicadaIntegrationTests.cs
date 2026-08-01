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

public sealed class RechazarOfertaDuplicadaIntegrationTests : IAsyncLifetime
{
    private const string MensajeOfertaDuplicada =
        "Este proveedor ya tiene una oferta registrada para esta licitación.";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("licitaciones_hu19_test")
        .WithUsername("hu19_test_user")
        .WithPassword("hu19_test_password")
        .Build();

    public Task InitializeAsync() => _postgres.StartAsync();

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task RegistrarSegundaOfertaMismoPar_RechazadaPorAplicacion_SinAfectarOriginal()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AplicarEsquemaCanonicoAsync();

        var (licitacionId, proveedorId) = await SembrarLicitacionYProveedorAsync(dbContext);
        var handler = scope.ServiceProvider.GetRequiredService<RegistrarOfertaHandler>();

        var primera = await handler.HandleAsync(
            new RegistrarOfertaCommand(licitacionId, proveedorId, 500_000m));

        var exception = await Assert.ThrowsAsync<OfertaDuplicadaException>(
            () => handler.HandleAsync(
                new RegistrarOfertaCommand(licitacionId, proveedorId, 400_000m)));

        Assert.Equal(MensajeOfertaDuplicada, exception.Message);

        dbContext.ChangeTracker.Clear();
        var ofertas = await dbContext.Ofertas.ToListAsync();
        Assert.Single(ofertas);
        Assert.Equal(primera.Id, ofertas[0].Id);
        Assert.Equal(500_000m, ofertas[0].MontoOfertadoCrc);
    }

    [Fact]
    public async Task InsertarSegundaOfertaMismoPar_RechazadaPorIndiceUnico()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AplicarEsquemaCanonicoAsync();

        var (licitacionId, proveedorId) = await SembrarLicitacionYProveedorAsync(dbContext);
        var writeRepository = scope.ServiceProvider
            .GetRequiredService<IOfertaWriteRepository>();
        var ahora = DateTimeOffset.UtcNow;

        await writeRepository.AgregarAsync(
            new Oferta(licitacionId, proveedorId, 500_000m, ahora));

        var exception = await Assert.ThrowsAsync<OfertaDuplicadaException>(
            () => writeRepository.AgregarAsync(
                new Oferta(licitacionId, proveedorId, 400_000m, ahora)));

        Assert.Equal(MensajeOfertaDuplicada, exception.Message);

        dbContext.ChangeTracker.Clear();
        var ofertas = await dbContext.Ofertas.ToListAsync();
        Assert.Single(ofertas);
        Assert.Equal(500_000m, ofertas[0].MontoOfertadoCrc);
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
        services.AddScoped<RegistrarOfertaHandler>();

        return services.BuildServiceProvider();
    }

    private static async Task<(Guid LicitacionId, Guid ProveedorId)>
        SembrarLicitacionYProveedorAsync(AppDbContext dbContext)
    {
        var ahora = DateTimeOffset.UtcNow;
        var licitacion = new Licitacion(
            "LIC-HU19",
            "Compra de suministros",
            ahora.AddDays(10),
            1_000_000m);
        licitacion.Publicar(ahora);
        var proveedor = new Proveedor("Proveedor HU19", "PROVEEDOR HU19");

        dbContext.AddRange(licitacion, proveedor);
        dbContext.Entry(licitacion)
            .Property<string>("CodigoNormalizado")
            .CurrentValue = licitacion.Codigo.ToUpperInvariant();
        await dbContext.SaveChangesAsync();

        return (licitacion.Id, proveedor.Id);
    }
}
