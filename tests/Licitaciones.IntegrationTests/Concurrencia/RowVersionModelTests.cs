using Licitaciones.Application.Common.Clock;
using Licitaciones.Domain.Entities;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Licitaciones.IntegrationTests.Concurrencia;

public sealed class RowVersionModelTests
{
    [Theory]
    [InlineData(typeof(Licitacion))]
    [InlineData(typeof(Proveedor))]
    [InlineData(typeof(Oferta))]
    [InlineData(typeof(NivelAprobacion))]
    [InlineData(typeof(TipoCambio))]
    public void EntidadEditable_ConfiguraRowVersionComoTokenGenerado(Type entityType)
    {
        using var context = CrearContexto();

        var property = context.Model.FindEntityType(entityType)?.FindProperty("RowVersion");

        Assert.NotNull(property);
        Assert.True(property.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
        Assert.Equal("row_version", property.GetColumnName());
    }

    private static AppDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options, new FixedClock());
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = DateTimeOffset.UnixEpoch;
    }
}
