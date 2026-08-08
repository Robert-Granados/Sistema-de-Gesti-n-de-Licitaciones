using Licitaciones.Domain.Enums;
using Licitaciones.Infrastructure;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Licitaciones.IntegrationTests.Postgres;

internal static class PostgresTestContext
{
    public static async Task AplicarMigracionesAsync(string connectionString)
    {
        await using var context = CrearContexto(connectionString);
        await context.Database.MigrateAsync();
    }

    public static AppDbContext CrearContexto(string connectionString)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MapEnum<EstadoLicitacion>(
                    "estado_licitacion",
                    nameTranslator: PreserveCaseNameTranslator.Instance))
            .Options;

        return new AppDbContext(options);
    }

    public static ServiceProvider CrearServicios(string connectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);

        return services.BuildServiceProvider();
    }

    public static async Task<(Guid LicitacionId, Guid ProveedorId)>
        SembrarLicitacionPublicadaYProveedorAsync(string connectionString)
    {
        var licitacionId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();
        var ahora = DateTimeOffset.UtcNow;

        await EjecutarAsync(connectionString, """
            INSERT INTO proveedores (id, nombre, nombre_normalizado)
            VALUES (@proveedorId, 'Proveedor HU45', 'PROVEEDOR HU45');
            """,
            new NpgsqlParameter("proveedorId", proveedorId));

        await EjecutarAsync(connectionString, """
            INSERT INTO licitaciones
                (id, codigo, titulo, estado, fecha_cierre, presupuesto_estimado_crc)
            VALUES (@licitacionId, 'LIC-HU45', 'Compra de suministros', 'Publicada',
                    @fechaCierre, 1000000.00);
            """,
            new NpgsqlParameter("licitacionId", licitacionId),
            new NpgsqlParameter("fechaCierre", ahora.AddDays(10)));

        return (licitacionId, proveedorId);
    }

    public static async Task EjecutarAsync(
        string connectionString,
        string sql,
        params NpgsqlParameter[] parametros)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await EjecutarAsync(connection, sql, parametros);
    }

    public static async Task EjecutarAsync(
        NpgsqlConnection connection,
        string sql,
        params NpgsqlParameter[] parametros) =>
        await EjecutarAsync(connection, null, sql, parametros);

    public static async Task EjecutarAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction,
        string sql,
        params NpgsqlParameter[] parametros)
    {
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddRange(parametros);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task<T> ConsultarEscalarAsync<T>(
        string connectionString,
        string sql,
        params NpgsqlParameter[] parametros)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        return await ConsultarEscalarAsync<T>(connection, sql, parametros);
    }

    public static async Task<T> ConsultarEscalarAsync<T>(
        NpgsqlConnection connection,
        string sql,
        params NpgsqlParameter[] parametros) =>
        await ConsultarEscalarAsync<T>(connection, null, sql, parametros);

    public static async Task<T> ConsultarEscalarAsync<T>(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction,
        string sql,
        params NpgsqlParameter[] parametros)
    {
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddRange(parametros);

        var resultado = await command.ExecuteScalarAsync();
        if (resultado is T valor)
        {
            return valor;
        }

        throw new InvalidOperationException(
            $"El resultado '{resultado}' no es del tipo esperado '{typeof(T)}'.");
    }

    public static async Task<List<string>> ConsultarColumnaAsync(
        string connectionString,
        string sql,
        params NpgsqlParameter[] parametros)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(parametros);

        var valores = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            valores.Add(reader.GetString(0));
        }

        return valores;
    }
}
