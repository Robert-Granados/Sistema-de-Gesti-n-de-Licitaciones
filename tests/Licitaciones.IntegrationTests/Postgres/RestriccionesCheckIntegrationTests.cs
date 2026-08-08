using Npgsql;

namespace Licitaciones.IntegrationTests.Postgres;

[Collection(PostgreSqlCollection.Name)]
public sealed class RestriccionesCheckIntegrationTests
{
    private readonly PostgreSqlCollectionFixture _postgres;

    public RestriccionesCheckIntegrationTests(PostgreSqlCollectionFixture postgres)
    {
        _postgres = postgres;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task OfertaConMontoNoPositivo_ViolaLaRestriccionCheck(decimal monto)
    {
        await using var baseAislada = await _postgres.CrearBaseAisladaAsync();

        await PostgresTestContext.AplicarMigracionesAsync(baseAislada.ConnectionString);
        var (licitacionId, proveedorId) = await PostgresTestContext
            .SembrarLicitacionPublicadaYProveedorAsync(baseAislada.ConnectionString);

        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            PostgresTestContext.EjecutarAsync(baseAislada.ConnectionString, """
                INSERT INTO ofertas (id, licitacion_id, proveedor_id, monto_ofertado_crc, fecha_registro)
                VALUES (@id, @licitacionId, @proveedorId, @monto, now());
                """,
                new NpgsqlParameter("id", Guid.NewGuid()),
                new NpgsqlParameter("licitacionId", licitacionId),
                new NpgsqlParameter("proveedorId", proveedorId),
                new NpgsqlParameter("monto", monto)));

        Assert.Equal(PostgresErrorCodes.CheckViolation, exception.SqlState);
        Assert.Equal("ck_ofertas_monto_positivo", exception.ConstraintName);

        Assert.Equal(
            0,
            await PostgresTestContext.ConsultarEscalarAsync<long>(
                baseAislada.ConnectionString,
                "SELECT COUNT(*) FROM ofertas"));
    }

    [Fact]
    public async Task LicitacionConPresupuestoNoPositivo_ViolaLaRestriccionCheck()
    {
        await using var baseAislada = await _postgres.CrearBaseAisladaAsync();

        await PostgresTestContext.AplicarMigracionesAsync(baseAislada.ConnectionString);

        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            PostgresTestContext.EjecutarAsync(baseAislada.ConnectionString, """
                INSERT INTO licitaciones
                    (id, codigo, titulo, estado, fecha_cierre, presupuesto_estimado_crc)
                VALUES (@id, 'LIC-CHECK', 'Licitación inválida', 'Borrador',
                        @fechaCierre, 0);
                """,
                new NpgsqlParameter("id", Guid.NewGuid()),
                new NpgsqlParameter("fechaCierre", DateTimeOffset.UtcNow.AddDays(10))));

        Assert.Equal(PostgresErrorCodes.CheckViolation, exception.SqlState);
        Assert.Equal("ck_licitaciones_presupuesto_positivo", exception.ConstraintName);

        Assert.Equal(
            0,
            await PostgresTestContext.ConsultarEscalarAsync<long>(
                baseAislada.ConnectionString,
                "SELECT COUNT(*) FROM licitaciones"));
    }
}
