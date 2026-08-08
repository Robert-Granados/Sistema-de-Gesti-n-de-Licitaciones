using Npgsql;

namespace Licitaciones.IntegrationTests.Postgres;

[Collection(PostgreSqlCollection.Name)]
public sealed class IndiceUnicoIntegrationTests
{
    private readonly PostgreSqlCollectionFixture _postgres;

    public IndiceUnicoIntegrationTests(PostgreSqlCollectionFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task OfertaDuplicada_MismoLicitacionYProveedor_ViolaElIndiceUnico()
    {
        await using var baseAislada = await _postgres.CrearBaseAisladaAsync();

        await PostgresTestContext.AplicarMigracionesAsync(baseAislada.ConnectionString);
        var (licitacionId, proveedorId) = await PostgresTestContext
            .SembrarLicitacionPublicadaYProveedorAsync(baseAislada.ConnectionString);

        await InsertarOfertaAsync(baseAislada.ConnectionString, licitacionId, proveedorId, 500_000m);

        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            InsertarOfertaAsync(baseAislada.ConnectionString, licitacionId, proveedorId, 400_000m));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, exception.SqlState);
        Assert.Equal("ux_ofertas_licitacion_proveedor", exception.ConstraintName);

        Assert.Equal(
            1,
            await PostgresTestContext.ConsultarEscalarAsync<long>(
                baseAislada.ConnectionString,
                "SELECT COUNT(*) FROM ofertas"));
    }

    [Fact]
    public async Task OfertaMismoParEnDistintaLicitacion_NoViolaElIndiceUnico()
    {
        await using var baseAislada = await _postgres.CrearBaseAisladaAsync();

        await PostgresTestContext.AplicarMigracionesAsync(baseAislada.ConnectionString);
        var (licitacionA, proveedorId) = await PostgresTestContext
            .SembrarLicitacionPublicadaYProveedorAsync(baseAislada.ConnectionString);

        var licitacionB = Guid.NewGuid();
        await PostgresTestContext.EjecutarAsync(baseAislada.ConnectionString, """
            INSERT INTO licitaciones
                (id, codigo, titulo, estado, fecha_cierre, presupuesto_estimado_crc)
            VALUES (@licitacionB, 'LIC-HU45-B', 'Segunda licitación', 'Publicada',
                    @fechaCierre, 1000000.00);
            """,
            new NpgsqlParameter("licitacionB", licitacionB),
            new NpgsqlParameter("fechaCierre", DateTimeOffset.UtcNow.AddDays(10)));

        await InsertarOfertaAsync(baseAislada.ConnectionString, licitacionA, proveedorId, 500_000m);
        await InsertarOfertaAsync(baseAislada.ConnectionString, licitacionB, proveedorId, 400_000m);

        Assert.Equal(
            2,
            await PostgresTestContext.ConsultarEscalarAsync<long>(
                baseAislada.ConnectionString,
                "SELECT COUNT(*) FROM ofertas"));
    }

    private static async Task InsertarOfertaAsync(
        string connectionString,
        Guid licitacionId,
        Guid proveedorId,
        decimal monto)
    {
        await PostgresTestContext.EjecutarAsync(connectionString, """
            INSERT INTO ofertas (id, licitacion_id, proveedor_id, monto_ofertado_crc, fecha_registro)
            VALUES (@id, @licitacionId, @proveedorId, @monto, now());
            """,
            new NpgsqlParameter("id", Guid.NewGuid()),
            new NpgsqlParameter("licitacionId", licitacionId),
            new NpgsqlParameter("proveedorId", proveedorId),
            new NpgsqlParameter("monto", monto));
    }
}
