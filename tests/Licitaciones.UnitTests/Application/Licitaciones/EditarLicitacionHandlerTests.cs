using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Licitaciones.Editar;
using Licitaciones.Application.Licitaciones.Exceptions;
using Licitaciones.Application.Licitaciones.Ports;
using Licitaciones.Domain.Entities;
using Licitaciones.Domain.Enums;

namespace Licitaciones.UnitTests.Application.Licitaciones;

public sealed class EditarLicitacionHandlerTests
{
    private static readonly FakeClock Clock = new(new(2026, 7, 24, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Handle_ConDatosValidos_ActualizaLicitacion()
    {
        var licitacion = new Licitacion("LIC-001", "Título original", Clock.UtcNow.AddDays(10), 1_000_000m);
        var repository = new FakeEditRepository(licitacion, rowVersion: 3, maxMontoOfertado: 0m);
        var handler = new EditarLicitacionHandler(repository, Clock);

        await handler.HandleAsync(new EditarLicitacionCommand(
            licitacion.Id,
            "Nuevo título",
            Clock.UtcNow.AddDays(20),
            2_000_000m,
            RowVersion: 3));

        Assert.Equal("Nuevo título", licitacion.Titulo);
        Assert.Equal(2_000_000m, licitacion.PresupuestoEstimadoCrc);
        Assert.Equal(3, repository.RowVersionGuardado);
    }

    [Fact]
    public async Task Handle_LicitacionCerrada_Rechaza()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Clock.UtcNow.AddDays(10), 1_000_000m);
        licitacion.Cerrar("Test", Clock.UtcNow);
        var repository = new FakeEditRepository(licitacion, rowVersion: 1, maxMontoOfertado: 0m);
        var handler = new EditarLicitacionHandler(repository, Clock);

        await Assert.ThrowsAsync<LicitacionCerradaException>(
            () => handler.HandleAsync(new EditarLicitacionCommand(
                licitacion.Id,
                "Nuevo título",
                Clock.UtcNow.AddDays(20),
                2_000_000m,
                RowVersion: 1)));

        Assert.False(repository.GuardarInvocado);
    }

    [Fact]
    public async Task Handle_FechaCierrePasada_Rechaza()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Clock.UtcNow.AddDays(-5), 1_000_000m);
        var repository = new FakeEditRepository(licitacion, rowVersion: 1, maxMontoOfertado: 0m);
        var handler = new EditarLicitacionHandler(repository, Clock);

        await Assert.ThrowsAsync<LicitacionCerradaException>(
            () => handler.HandleAsync(new EditarLicitacionCommand(
                licitacion.Id,
                "Nuevo título",
                Clock.UtcNow.AddDays(20),
                2_000_000m,
                RowVersion: 1)));

        Assert.False(repository.GuardarInvocado);
    }

    [Fact]
    public async Task Handle_PresupuestoMenorAlMaxMontoOferta_Rechaza()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Clock.UtcNow.AddDays(10), 1_000_000m);
        var repository = new FakeEditRepository(licitacion, rowVersion: 1, maxMontoOfertado: 800_000m);
        var handler = new EditarLicitacionHandler(repository, Clock);

        var exception = await Assert.ThrowsAsync<PresupuestoInsuficienteException>(
            () => handler.HandleAsync(new EditarLicitacionCommand(
                licitacion.Id,
                "Nuevo título",
                Clock.UtcNow.AddDays(20),
                700_000m,
                RowVersion: 1)));

        Assert.Contains("800", exception.Message);
        Assert.False(repository.GuardarInvocado);
    }

    [Fact]
    public async Task Handle_PresupuestoIgualAlMaxMontoOferta_Acepta()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Clock.UtcNow.AddDays(10), 1_000_000m);
        var repository = new FakeEditRepository(licitacion, rowVersion: 1, maxMontoOfertado: 800_000m);
        var handler = new EditarLicitacionHandler(repository, Clock);

        await handler.HandleAsync(new EditarLicitacionCommand(
            licitacion.Id,
            "Nuevo título",
            Clock.UtcNow.AddDays(20),
            800_000m,
            RowVersion: 1));

        Assert.True(repository.GuardarInvocado);
    }

    [Fact]
    public async Task Handle_LicitacionInexistente_Rechaza()
    {
        var repository = new FakeEditRepository(null, rowVersion: 0, maxMontoOfertado: 0m);
        var handler = new EditarLicitacionHandler(repository, Clock);

        await Assert.ThrowsAsync<LicitacionNoEncontradaException>(
            () => handler.HandleAsync(new EditarLicitacionCommand(
                Guid.NewGuid(),
                "Nuevo título",
                Clock.UtcNow.AddDays(20),
                1_000_000m,
                RowVersion: 1)));
    }

    [Fact]
    public async Task ObtenerAsync_ConIdValido_RetornaDto()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Clock.UtcNow.AddDays(10), 1_000_000m);
        var repository = new FakeEditRepository(licitacion, rowVersion: 5, maxMontoOfertado: 0m);
        var handler = new EditarLicitacionHandler(repository, Clock);

        var result = await handler.ObtenerAsync(licitacion.Id);

        Assert.NotNull(result);
        Assert.Equal("LIC-001", result.Codigo);
        Assert.Equal(5, result.RowVersion);
    }

    [Fact]
    public async Task ObtenerAsync_ConIdVacio_RetornaNull()
    {
        var repository = new FakeEditRepository(null, rowVersion: 0, maxMontoOfertado: 0m);
        var handler = new EditarLicitacionHandler(repository, Clock);

        var result = await handler.ObtenerAsync(Guid.Empty);

        Assert.Null(result);
    }

    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }

    private sealed class FakeEditRepository(
        Licitacion? licitacion,
        int rowVersion,
        decimal maxMontoOfertado) : ILicitacionEditRepository
    {
        public bool GuardarInvocado { get; private set; }
        public int? RowVersionGuardado { get; private set; }

        public Task<LicitacionEdicion?> ObtenerPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (licitacion is null || licitacion.Id != id)
            {
                return Task.FromResult<LicitacionEdicion?>(null);
            }

            return Task.FromResult<LicitacionEdicion?>(
                new LicitacionEdicion(licitacion, rowVersion));
        }

        public Task<decimal> ObtenerMaxMontoOfertadoAsync(
            Guid licitacionId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(maxMontoOfertado);

        public Task GuardarAsync(
            Licitacion licitacionActualizada,
            int expectedRowVersion,
            CancellationToken cancellationToken = default)
        {
            GuardarInvocado = true;
            RowVersionGuardado = expectedRowVersion;
            return Task.CompletedTask;
        }
    }
}
