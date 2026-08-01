using Licitaciones.Application.Ofertas.Listar;
using Licitaciones.Application.Ofertas.Ports;
using Licitaciones.Domain.Entities;
using Licitaciones.Infrastructure;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Licitaciones.IntegrationTests.Ofertas;

public sealed class ListarOfertasIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("licitaciones_hu22_test")
        .WithUsername("hu22_test_user")
        .WithPassword("hu22_test_password")
        .Build();

    public Task InitializeAsync() => _postgres.StartAsync();

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task FiltrarPorLicitacion_DevuelveSoloSusOfertasOrdenadasPorMonto()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AplicarEsquemaCanonicoAsync();

        var datos = await SembrarAsync(dbContext);
        var handler = scope.ServiceProvider.GetRequiredService<ListarOfertasHandler>();

        var pagina = await handler.HandleAsync(new ListarOfertasQuery(
            PageSize: 100,
            LicitacionId: datos.LicitacionA));

        Assert.Equal(3, pagina.TotalRegistros);
        Assert.Equal(
            new[] { 200_000m, 300_000m, 500_000m },
            pagina.Elementos.Select(o => o.MontoOfertadoCrc));
        Assert.All(pagina.Elementos, o => Assert.Equal("LIC-A", o.CodigoLicitacion));
    }

    [Fact]
    public async Task FiltrarPorProveedor_DevuelveSusOfertasEnTodasLasLicitaciones()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AplicarEsquemaCanonicoAsync();

        var datos = await SembrarAsync(dbContext);
        var handler = scope.ServiceProvider.GetRequiredService<ListarOfertasHandler>();

        var pagina = await handler.HandleAsync(new ListarOfertasQuery(
            PageSize: 100,
            ProveedorId: datos.Proveedor1));

        Assert.Equal(2, pagina.TotalRegistros);
        Assert.Equal(
            new[] { 300_000m, 400_000m },
            pagina.Elementos.Select(o => o.MontoOfertadoCrc));
        Assert.All(pagina.Elementos, o => Assert.Equal("Proveedor 1", o.NombreProveedor));
    }

    [Fact]
    public async Task FiltrarPorLicitacionYProveedor_CombinaAmbosFiltros()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AplicarEsquemaCanonicoAsync();

        var datos = await SembrarAsync(dbContext);
        var handler = scope.ServiceProvider.GetRequiredService<ListarOfertasHandler>();

        var pagina = await handler.HandleAsync(new ListarOfertasQuery(
            LicitacionId: datos.LicitacionA,
            ProveedorId: datos.Proveedor1));

        Assert.Equal(1, pagina.TotalRegistros);
        var oferta = Assert.Single(pagina.Elementos);
        Assert.Equal(300_000m, oferta.MontoOfertadoCrc);
        Assert.Equal(datos.LicitacionA, oferta.LicitacionId);
        Assert.Equal(datos.Proveedor1, oferta.ProveedorId);
    }

    [Fact]
    public async Task OrdenarPorFechaDescendente_DevuelveMasRecientesPrimero()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AplicarEsquemaCanonicoAsync();

        var datos = await SembrarAsync(dbContext);
        var handler = scope.ServiceProvider.GetRequiredService<ListarOfertasHandler>();

        var pagina = await handler.HandleAsync(new ListarOfertasQuery(
            PageSize: 100,
            SortBy: "fecha_desc"));

        Assert.Equal(5, pagina.TotalRegistros);
        var fechas = pagina.Elementos.Select(o => o.FechaRegistro).ToList();
        Assert.Equal(
            fechas.OrderByDescending(fecha => fecha),
            fechas);
    }

    [Fact]
    public async Task Paginar_DevuelveSubconjuntoConTotalesCorrectos()
    {
        await using var services = CrearServicios();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AplicarEsquemaCanonicoAsync();

        var datos = await SembrarAsync(dbContext);
        var handler = scope.ServiceProvider.GetRequiredService<ListarOfertasHandler>();

        var primera = await handler.HandleAsync(new ListarOfertasQuery(Page: 1, PageSize: 2));
        var segunda = await handler.HandleAsync(new ListarOfertasQuery(Page: 2, PageSize: 2));

        Assert.Equal(5, primera.TotalRegistros);
        Assert.Equal(3, primera.TotalPaginas);
        Assert.Equal(new[] { 200_000m, 300_000m }, primera.Elementos.Select(o => o.MontoOfertadoCrc));
        Assert.Equal(new[] { 400_000m, 500_000m }, segunda.Elementos.Select(o => o.MontoOfertadoCrc));
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
        services.AddScoped<ListarOfertasHandler>();

        return services.BuildServiceProvider();
    }

    private static async Task<DatosSembrados> SembrarAsync(AppDbContext dbContext)
    {
        var ahora = DateTimeOffset.UtcNow;
        var licitacionA = new Licitacion(
            "LIC-A", "Compra de suministros", ahora.AddDays(10), 1_000_000m);
        licitacionA.Publicar(ahora);
        var licitacionB = new Licitacion(
            "LIC-B", "Compra de equipos", ahora.AddDays(10), 2_000_000m);
        licitacionB.Publicar(ahora);

        var proveedor1 = new Proveedor("Proveedor 1");
        var proveedor2 = new Proveedor("Proveedor 2");
        var proveedor3 = new Proveedor("Proveedor 3");

        dbContext.AddRange(licitacionA, licitacionB, proveedor1, proveedor2, proveedor3);
        dbContext.Entry(licitacionA)
            .Property<string>("CodigoNormalizado")
            .CurrentValue = licitacionA.Codigo.ToUpperInvariant();
        dbContext.Entry(licitacionB)
            .Property<string>("CodigoNormalizado")
            .CurrentValue = licitacionB.Codigo.ToUpperInvariant();
        await dbContext.SaveChangesAsync();

        var ofertas = new[]
        {
            new Oferta(licitacionA.Id, proveedor1.Id, 300_000m, ahora.AddMinutes(1)),
            new Oferta(licitacionA.Id, proveedor2.Id, 500_000m, ahora.AddMinutes(2)),
            new Oferta(licitacionA.Id, proveedor3.Id, 200_000m, ahora.AddMinutes(3)),
            new Oferta(licitacionB.Id, proveedor1.Id, 400_000m, ahora.AddMinutes(4)),
            new Oferta(licitacionB.Id, proveedor2.Id, 600_000m, ahora.AddMinutes(5)),
        };

        dbContext.AddRange(ofertas);
        await dbContext.SaveChangesAsync();

        return new DatosSembrados(
            licitacionA.Id,
            licitacionB.Id,
            proveedor1.Id,
            proveedor2.Id,
            proveedor3.Id);
    }

    private sealed record DatosSembrados(
        Guid LicitacionA,
        Guid LicitacionB,
        Guid Proveedor1,
        Guid Proveedor2,
        Guid Proveedor3);
}
