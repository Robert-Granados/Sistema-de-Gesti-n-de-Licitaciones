using Npgsql;
using Testcontainers.PostgreSql;

namespace Licitaciones.IntegrationTests.Postgres;

public sealed class PostgreSqlCollectionFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("licitaciones_hu45")
        .WithUsername("hu45_test_user")
        .WithPassword("hu45_test_password")
        .Build();

    public Task InitializeAsync() => _postgres.StartAsync();

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    public async Task<BasePostgreSqlAislada> CrearBaseAisladaAsync()
    {
        var nombre = $"hu45_{Guid.NewGuid():N}";

        await using var connection = new NpgsqlConnection(CadenaAdministracion);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE DATABASE \"{nombre}\"", connection);
        await command.ExecuteNonQueryAsync();

        return new BasePostgreSqlAislada(this, nombre);
    }

    public Task EliminarBaseAsync(string nombre) =>
        EjecutarAdministracionAsync($"DROP DATABASE IF EXISTS \"{nombre}\" WITH (FORCE)");

    public string ObtenerCadenaDeConexion(string nombre) =>
        new NpgsqlConnectionStringBuilder(_postgres.GetConnectionString())
        {
            Database = nombre,
        }.ConnectionString;

    private string CadenaAdministracion =>
        new NpgsqlConnectionStringBuilder(_postgres.GetConnectionString())
        {
            Database = "postgres",
        }.ConnectionString;

    private async Task EjecutarAdministracionAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(CadenaAdministracion);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}

public sealed class BasePostgreSqlAislada : IAsyncDisposable
{
    private readonly PostgreSqlCollectionFixture _fixture;

    public BasePostgreSqlAislada(PostgreSqlCollectionFixture fixture, string nombre)
    {
        _fixture = fixture;
        Nombre = nombre;
    }

    public string Nombre { get; }

    public string ConnectionString => _fixture.ObtenerCadenaDeConexion(Nombre);

    public ValueTask DisposeAsync() => new(_fixture.EliminarBaseAsync(Nombre));
}
