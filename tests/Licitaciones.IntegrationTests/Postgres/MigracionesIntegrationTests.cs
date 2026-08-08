using Npgsql;

namespace Licitaciones.IntegrationTests.Postgres;

[Collection(PostgreSqlCollection.Name)]
public sealed class MigracionesIntegrationTests
{
    private readonly PostgreSqlCollectionFixture _postgres;

    public MigracionesIntegrationTests(PostgreSqlCollectionFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task AplicarMigraciones_RegistraLasMigracionesYDejaElEsquemaCompleto()
    {
        await using var baseAislada = await _postgres.CrearBaseAisladaAsync();

        await PostgresTestContext.AplicarMigracionesAsync(baseAislada.ConnectionString);

        var migraciones = await PostgresTestContext.ConsultarColumnaAsync(
            connectionString: baseAislada.ConnectionString,
            sql: @"SELECT ""MigrationId"" FROM ""__EFMigrationsHistory"" ORDER BY ""MigrationId""");

        Assert.Equal(
            new[]
            {
                "20260724011210_InitialCreate",
                "20260807120000_HU40AuditCreatedAtImmutable",
                "20260807150000_HU43LicitacionLifecycleColumns",
            },
            migraciones);

        var tablas = await PostgresTestContext.ConsultarColumnaAsync(
            baseAislada.ConnectionString,
            "SELECT tablename FROM pg_tables WHERE schemaname = 'public' ORDER BY tablename");

        Assert.Contains("licitaciones", tablas);
        Assert.Contains("proveedores", tablas);
        Assert.Contains("ofertas", tablas);
        Assert.Contains("niveles_aprobacion", tablas);
        Assert.Contains("tipos_cambio", tablas);

        Assert.Equal(
            3,
            await PostgresTestContext.ConsultarEscalarAsync<long>(
                baseAislada.ConnectionString,
                "SELECT COUNT(*) FROM niveles_aprobacion"));

        Assert.Equal(
            1,
            await PostgresTestContext.ConsultarEscalarAsync<long>(
                baseAislada.ConnectionString,
                "SELECT COUNT(*) FROM tipos_cambio WHERE activo = true"));
    }

    [Fact]
    public async Task AplicarMigraciones_CreaIndicesRestriccionesYTriggers()
    {
        await using var baseAislada = await _postgres.CrearBaseAisladaAsync();

        await PostgresTestContext.AplicarMigracionesAsync(baseAislada.ConnectionString);

        await using var connection = new NpgsqlConnection(baseAislada.ConnectionString);
        await connection.OpenAsync();

        Assert.True(await ExisteIndiceAsync(connection, "ofertas", "ux_ofertas_licitacion_proveedor"));
        Assert.True(await ExisteIndiceAsync(connection, "licitaciones", "ux_licitaciones_codigo_normalizado"));
        Assert.True(await ExisteIndiceAsync(connection, "proveedores", "ux_proveedores_nombre_normalizado"));

        Assert.True(await ExisteRestriccionAsync(connection, "ofertas", "fk_ofertas_licitacion"));
        Assert.True(await ExisteRestriccionAsync(connection, "ofertas", "fk_ofertas_proveedor"));
        Assert.True(await ExisteRestriccionAsync(connection, "ofertas", "ck_ofertas_monto_positivo"));
        Assert.True(await ExisteRestriccionAsync(connection, "licitaciones", "ck_licitaciones_presupuesto_positivo"));
        Assert.True(await ExisteRestriccionAsync(connection, "niveles_aprobacion", "ex_niveles_rango_sin_traslape"));

        Assert.True(await ExisteTriggerAsync(connection, "ofertas", "trg_ofertas_validar_negocio"));
        Assert.True(await ExisteTriggerAsync(connection, "proveedores", "trg_proveedores_normalizar"));
    }

    [Fact]
    public async Task AplicarMigraciones_RegistraLaColumnaRowVersionEnLasEntidades()
    {
        await using var baseAislada = await _postgres.CrearBaseAisladaAsync();

        await PostgresTestContext.AplicarMigracionesAsync(baseAislada.ConnectionString);

        foreach (var tabla in new[] { "licitaciones", "proveedores", "ofertas" })
        {
            Assert.Equal(
                1,
                await PostgresTestContext.ConsultarEscalarAsync<long>(
                    baseAislada.ConnectionString,
                    """
                    SELECT COUNT(*) FROM information_schema.columns
                     WHERE table_name = @tabla
                       AND column_name = 'row_version'
                    """,
                    new NpgsqlParameter("tabla", tabla)));
        }
    }

    private static async Task<bool> ExisteIndiceAsync(
        NpgsqlConnection connection,
        string tabla,
        string indice)
    {
        var resultado = await PostgresTestContext.ConsultarEscalarAsync<long>(
            connection,
            """
            SELECT COUNT(*) FROM pg_indexes
             WHERE schemaname = 'public'
               AND tablename = @tabla
               AND indexname = @indice
            """,
            new NpgsqlParameter("tabla", tabla),
            new NpgsqlParameter("indice", indice));

        return resultado == 1;
    }

    private static async Task<bool> ExisteRestriccionAsync(
        NpgsqlConnection connection,
        string tabla,
        string restriccion)
    {
        var resultado = await PostgresTestContext.ConsultarEscalarAsync<long>(
            connection,
            """
            SELECT COUNT(*) FROM pg_constraint
             WHERE conrelid = @tabla::regclass
               AND conname = @restriccion
            """,
            new NpgsqlParameter("tabla", tabla),
            new NpgsqlParameter("restriccion", restriccion));

        return resultado == 1;
    }

    private static async Task<bool> ExisteTriggerAsync(
        NpgsqlConnection connection,
        string tabla,
        string trigger)
    {
        var resultado = await PostgresTestContext.ConsultarEscalarAsync<long>(
            connection,
            """
            SELECT COUNT(*) FROM information_schema.triggers
             WHERE event_object_table = @tabla
               AND trigger_name = @trigger
            """,
            new NpgsqlParameter("tabla", tabla),
            new NpgsqlParameter("trigger", trigger));

        return resultado > 0;
    }
}
