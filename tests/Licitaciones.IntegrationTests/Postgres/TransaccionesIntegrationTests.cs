using Npgsql;

namespace Licitaciones.IntegrationTests.Postgres;

[Collection(PostgreSqlCollection.Name)]
public sealed class TransaccionesIntegrationTests
{
    private readonly PostgreSqlCollectionFixture _postgres;

    public TransaccionesIntegrationTests(PostgreSqlCollectionFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task TransaccionMultiRegistro_ConRegistrosValidos_SeConfirmaAtomica()
    {
        await using var baseAislada = await _postgres.CrearBaseAisladaAsync();

        await PostgresTestContext.AplicarMigracionesAsync(baseAislada.ConnectionString);
        var licitacionId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();
        var ofertaId = Guid.NewGuid();

        await using var connection = new NpgsqlConnection(baseAislada.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await PostgresTestContext.EjecutarAsync(connection, transaction, """
            INSERT INTO proveedores (id, nombre, nombre_normalizado)
            VALUES (@proveedorId, 'Proveedor Transacción', 'PROVEEDOR TRANSACCION');
            """,
            new NpgsqlParameter("proveedorId", proveedorId));

        await PostgresTestContext.EjecutarAsync(connection, transaction, """
            INSERT INTO licitaciones
                (id, codigo, titulo, estado, fecha_cierre, presupuesto_estimado_crc)
            VALUES (@licitacionId, 'LIC-TX', 'Compra transaccional', 'Publicada',
                    @fechaCierre, 1000000.00);
            """,
            new NpgsqlParameter("licitacionId", licitacionId),
            new NpgsqlParameter("fechaCierre", DateTimeOffset.UtcNow.AddDays(10)));

        await PostgresTestContext.EjecutarAsync(connection, transaction, """
            INSERT INTO ofertas (id, licitacion_id, proveedor_id, monto_ofertado_crc, fecha_registro)
            VALUES (@ofertaId, @licitacionId, @proveedorId, 250000.00, now());
            """,
            new NpgsqlParameter("ofertaId", ofertaId),
            new NpgsqlParameter("licitacionId", licitacionId),
            new NpgsqlParameter("proveedorId", proveedorId));

        await transaction.CommitAsync();

        Assert.Equal(
            1,
            await PostgresTestContext.ConsultarEscalarAsync<long>(
                baseAislada.ConnectionString,
                "SELECT COUNT(*) FROM licitaciones"));
        Assert.Equal(
            1,
            await PostgresTestContext.ConsultarEscalarAsync<long>(
                baseAislada.ConnectionString,
                "SELECT COUNT(*) FROM proveedores"));
        Assert.Equal(
            1,
            await PostgresTestContext.ConsultarEscalarAsync<long>(
                baseAislada.ConnectionString,
                "SELECT COUNT(*) FROM ofertas WHERE id = @id",
                new NpgsqlParameter("id", ofertaId)));
    }

    [Fact]
    public async Task TransaccionMultiRegistro_ConRegistroInvalido_SeRevierteCompleta()
    {
        await using var baseAislada = await _postgres.CrearBaseAisladaAsync();

        await PostgresTestContext.AplicarMigracionesAsync(baseAislada.ConnectionString);
        var licitacionId = Guid.NewGuid();

        await using var connection = new NpgsqlConnection(baseAislada.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await PostgresTestContext.EjecutarAsync(connection, transaction, """
            INSERT INTO licitaciones
                (id, codigo, titulo, estado, fecha_cierre, presupuesto_estimado_crc)
            VALUES (@licitacionId, 'LIC-TX-FALLIDA', 'Compra fallida', 'Publicada',
                    @fechaCierre, 1000000.00);
            """,
            new NpgsqlParameter("licitacionId", licitacionId),
            new NpgsqlParameter("fechaCierre", DateTimeOffset.UtcNow.AddDays(10)));

        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            PostgresTestContext.EjecutarAsync(connection, transaction, """
                INSERT INTO ofertas (id, licitacion_id, proveedor_id, monto_ofertado_crc, fecha_registro)
                VALUES (@id, @licitacionId, @proveedorInexistente, 250000.00, now());
                """,
                new NpgsqlParameter("id", Guid.NewGuid()),
                new NpgsqlParameter("licitacionId", licitacionId),
                new NpgsqlParameter("proveedorInexistente", Guid.NewGuid())));

        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, exception.SqlState);

        await transaction.RollbackAsync();

        Assert.Equal(
            0,
            await PostgresTestContext.ConsultarEscalarAsync<long>(
                baseAislada.ConnectionString,
                "SELECT COUNT(*) FROM licitaciones"));
        Assert.Equal(
            0,
            await PostgresTestContext.ConsultarEscalarAsync<long>(
                baseAislada.ConnectionString,
                "SELECT COUNT(*) FROM ofertas"));
    }
}
