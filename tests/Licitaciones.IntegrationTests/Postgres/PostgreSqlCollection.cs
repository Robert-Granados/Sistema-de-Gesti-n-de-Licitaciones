namespace Licitaciones.IntegrationTests.Postgres;

[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlCollectionFixture>
{
    public const string Name = "PostgreSQL real (Testcontainers)";
}
