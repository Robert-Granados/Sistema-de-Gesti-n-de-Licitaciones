using Licitaciones.Domain.Entities;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Licitaciones.IntegrationTests.Postgres;

[Collection(PostgreSqlCollection.Name)]
public sealed class ConcurrenciaOptimistaIntegrationTests
{
    private readonly PostgreSqlCollectionFixture _postgres;

    public ConcurrenciaOptimistaIntegrationTests(PostgreSqlCollectionFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task DosContextosActualizanLaMismaEntidad_SegundoGeneraConflictoDeConcurrencia()
    {
        await using var baseAislada = await _postgres.CrearBaseAisladaAsync();

        await PostgresTestContext.AplicarMigracionesAsync(baseAislada.ConnectionString);

        await using var contextoA = PostgresTestContext.CrearContexto(baseAislada.ConnectionString);
        await using var contextoB = PostgresTestContext.CrearContexto(baseAislada.ConnectionString);

        var proveedor = new Proveedor("Proveedor Concurrencia", "PROVEEDOR CONCURRENCIA");
        contextoA.Proveedores.Add(proveedor);
        await contextoA.SaveChangesAsync();

        var proveedorVistoPorB = await contextoB.Proveedores
            .SingleAsync(item => item.Id == proveedor.Id);

        var proveedorVistoPorA = await contextoA.Proveedores
            .SingleAsync(item => item.Id == proveedor.Id);
        proveedorVistoPorA.CambiarNombre("Proveedor Editado A", "PROVEEDOR EDITADO A");
        await contextoA.SaveChangesAsync();

        proveedorVistoPorB.CambiarNombre("Proveedor Editado B", "PROVEEDOR EDITADO B");
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => contextoB.SaveChangesAsync());

        var nombrePersistido = await PostgresTestContext.ConsultarEscalarAsync<string>(
            baseAislada.ConnectionString,
            "SELECT nombre FROM proveedores WHERE id = @id",
            new NpgsqlParameter("id", proveedor.Id));

        Assert.Equal("Proveedor Editado A", nombrePersistido);
    }
}
