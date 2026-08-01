using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Licitaciones.Exceptions;
using Licitaciones.Application.Licitaciones.Ports;
using Licitaciones.Application.Licitaciones.Publicar;
using Licitaciones.Domain.Entities;
using Licitaciones.Domain.Enums;
using Licitaciones.Domain.Exceptions;

namespace Licitaciones.UnitTests.Application.Licitaciones;

public sealed class PublicarLicitacionHandlerTests
{
    private static readonly FakeClock Clock = new(new(2026, 7, 24, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Handle_LicitacionBorradorValida_Publica()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Clock.UtcNow.AddDays(10), 1_000_000m);
        var repository = new FakeEditRepository(licitacion, rowVersion: 1);
        var handler = new PublicarLicitacionHandler(repository, Clock);

        var result = await handler.HandleAsync(
            new PublicarLicitacionCommand(licitacion.Id));

        Assert.Equal(licitacion.Id, result.Id);
        Assert.Equal("LIC-001", result.Codigo);
        Assert.Equal(EstadoLicitacion.Publicada, licitacion.Estado);
        Assert.True(repository.GuardarInvocado);
    }

    [Fact]
    public async Task Handle_LicitacionPublicada_Rechaza()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Clock.UtcNow.AddDays(10), 1_000_000m);
        licitacion.Publicar(Clock.UtcNow);
        var repository = new FakeEditRepository(licitacion, rowVersion: 2);
        var handler = new PublicarLicitacionHandler(repository, Clock);

        await Assert.ThrowsAsync<TransicionEstadoInvalidaException>(
            () => handler.HandleAsync(new PublicarLicitacionCommand(licitacion.Id)));

        Assert.False(repository.GuardarInvocado);
    }

    [Fact]
    public async Task Handle_LicitacionCerrada_Rechaza()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Clock.UtcNow.AddDays(10), 1_000_000m);
        licitacion.Cerrar("Cancelación", Clock.UtcNow);
        var repository = new FakeEditRepository(licitacion, rowVersion: 3);
        var handler = new PublicarLicitacionHandler(repository, Clock);

        await Assert.ThrowsAsync<TransicionEstadoInvalidaException>(
            () => handler.HandleAsync(new PublicarLicitacionCommand(licitacion.Id)));

        Assert.False(repository.GuardarInvocado);
    }

    [Fact]
    public async Task Handle_LicitacionConFechaPasada_Rechaza()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Clock.UtcNow.AddDays(-5), 1_000_000m);
        var repository = new FakeEditRepository(licitacion, rowVersion: 1);
        var handler = new PublicarLicitacionHandler(repository, Clock);

        await Assert.ThrowsAsync<TransicionEstadoInvalidaException>(
            () => handler.HandleAsync(new PublicarLicitacionCommand(licitacion.Id)));

        Assert.False(repository.GuardarInvocado);
    }

    [Fact]
    public async Task Handle_LicitacionInexistente_Rechaza()
    {
        var repository = new FakeEditRepository(null, rowVersion: 0);
        var handler = new PublicarLicitacionHandler(repository, Clock);

        await Assert.ThrowsAsync<LicitacionNoEncontradaException>(
            () => handler.HandleAsync(new PublicarLicitacionCommand(Guid.NewGuid())));
    }

    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }

    private sealed class FakeEditRepository(
        Licitacion? licitacion,
        int rowVersion) : ILicitacionEditRepository
    {
        public bool GuardarInvocado { get; private set; }

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
            Task.FromResult(0m);

        public Task GuardarAsync(
            Licitacion licitacionActualizada,
            int expectedRowVersion,
            CancellationToken cancellationToken = default)
        {
            GuardarInvocado = true;
            return Task.CompletedTask;
        }
    }
}
