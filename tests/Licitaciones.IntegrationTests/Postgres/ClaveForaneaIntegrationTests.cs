using Npgsql;

namespace Licitaciones.IntegrationTests.Postgres;

[Collection(PostgreSqlCollection.Name)]
public sealed class ClaveForaneaIntegrationTests
{
    private readonly PostgreSqlCollectionFixture _postgres;

    public ClaveForaneaIntegrationTests(PostgreSqlCollectionFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task OfertaConProveedorInexistente_ViolaLaClaveForanea()
    {
        await using var baseAislada = await _postgres.CrearBaseAisladaAsync();

        await PostgresTestContext.AplicarMigracionesAsync(baseAislada.ConnectionString);
        var (licitacionId, _) = await PostgresTestContext
            .SembrarLicitacionPublicadaYProveedorAsync(baseAislada.ConnectionString);

        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            PostgresTestContext.EjecutarAsync(baseAislada.ConnectionString, """
                INSERT INTO ofertas (id, licitacion_id, proveedor_id, monto_ofertado_crc, fecha_registro)
                VALUES (@id, @licitacionId, @proveedorInexistente, 500000.00, now());
                """,
                new NpgsqlParameter("id", Guid.NewGuid()),
                new NpgsqlParameter("licitacionId", licitacionId),
                new NpgsqlParameter("proveedorInexistente", Guid.NewGuid())));

        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, exception.SqlState);
        Assert.Equal("fk_ofertas_proveedor", exception.ConstraintName);

        Assert.Equal(
            0,
            await PostgresTestContext.ConsultarEscalarAsync<long>(
                baseAislada.ConnectionString,
                "SELECT COUNT(*) FROM ofertas"));
    }

    [Fact]
    public async Task OfertaConLicitacionInexistente_ViolaLaClaveForanea()
    {
        await using var baseAislada = await _postgres.CrearBaseAisladaAsync();

        await PostgresTestContext.AplicarMigracionesAsync(baseAislada.ConnectionString);
        var (_, proveedorId) = await PostgresTestContext
            .SembrarLicitacionPublicadaYProveedorAsync(baseAislada.ConnectionString);

        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            PostgresTestContext.EjecutarAsync(baseAislada.ConnectionString, """
                INSERT INTO ofertas (id, licitacion_id, proveedor_id, monto_ofertado_crc, fecha_registro)
                VALUES (@id, @licitacionInexistente, @proveedorId, 500000.00, now());
                """,
                new NpgsqlParameter("id", Guid.NewGuid()),
                new NpgsqlParameter("licitacionInexistente", Guid.NewGuid()),
                new NpgsqlParameter("proveedorId", proveedorId)));

        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, exception.SqlState);

        Assert.Equal(
            0,
            await PostgresTestContext.ConsultarEscalarAsync<long>(
                baseAislada.ConnectionString,
                "SELECT COUNT(*) FROM ofertas"));
    }
}
