using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Licitaciones.Cerrar;
using Licitaciones.Application.Licitaciones.Exceptions;
using Licitaciones.Application.Licitaciones.Ports;
using Licitaciones.Domain.Entities;
using Licitaciones.Domain.Enums;
using Licitaciones.Domain.Exceptions;

namespace Licitaciones.UnitTests.Application.Licitaciones;

public sealed class CerrarLicitacionHandlerTests
{
    private static readonly FakeClock Clock = new(new(2026, 7, 24, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Handle_LicitacionPublicada_ConMotivo_Cierra()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Clock.UtcNow.AddDays(10), 1_000_000m);
        licitacion.Publicar(Clock.UtcNow);
        var repository = new FakeEditRepository(licitacion, rowVersion: 2);
        var handler = new CerrarLicitacionHandler(repository, Clock);

        var result = await handler.HandleAsync(
            new CerrarLicitacionCommand(licitacion.Id, "Fecha de cierre alcanzada"));

        Assert.Equal(licitacion.Id, result.Id);
        Assert.Equal("LIC-001", result.Codigo);
        Assert.Equal(EstadoLicitacion.Cerrada, licitacion.Estado);
        Assert.Equal("Fecha de cierre alcanzada", licitacion.MotivoCierre);
        Assert.True(repository.GuardarInvocado);
    }

    [Fact]
    public async Task Handle_LicitacionBorrador_ConMotivo_Cierra()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Clock.UtcNow.AddDays(10), 1_000_000m);
        var repository = new FakeEditRepository(licitacion, rowVersion: 1);
        var handler = new CerrarLicitacionHandler(repository, Clock);

        var result = await handler.HandleAsync(
            new CerrarLicitacionCommand(licitacion.Id, "Cancelación manual"));

        Assert.Equal(licitacion.Id, result.Id);
        Assert.Equal(EstadoLicitacion.Cerrada, licitacion.Estado);
        Assert.Equal("Cancelación manual", licitacion.MotivoCierre);
        Assert.True(repository.GuardarInvocado);
    }

    [Fact]
    public async Task Handle_LicitacionYaCerrada_Rechaza()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Clock.UtcNow.AddDays(10), 1_000_000m);
        licitacion.Cerrar("Primera razón", Clock.UtcNow);
        var repository = new FakeEditRepository(licitacion, rowVersion: 3);
        var handler = new CerrarLicitacionHandler(repository, Clock);

        await Assert.ThrowsAsync<LicitacionCerradaException>(
            () => handler.HandleAsync(
                new CerrarLicitacionCommand(licitacion.Id, "Segunda razón")));

        Assert.False(repository.GuardarInvocado);
    }

    [Fact]
    public async Task Handle_LicitacionInexistente_Rechaza()
    {
        var repository = new FakeEditRepository(null, rowVersion: 0);
        var handler = new CerrarLicitacionHandler(repository, Clock);

        await Assert.ThrowsAsync<LicitacionNoEncontradaException>(
            () => handler.HandleAsync(
                new CerrarLicitacionCommand(Guid.NewGuid(), "Motivo")));
    }

    [Fact]
    public async Task Handle_ConMotivoVacio_Rechaza()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Clock.UtcNow.AddDays(10), 1_000_000m);
        var repository = new FakeEditRepository(licitacion, rowVersion: 1);
        var handler = new CerrarLicitacionHandler(repository, Clock);

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.HandleAsync(
                new CerrarLicitacionCommand(licitacion.Id, "")));
    }

    [Fact]
    public async Task Handle_TransicionBorradorACerrada_RegistraMotivoYFecha()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Clock.UtcNow.AddDays(10), 1_000_000m);
        var repository = new FakeEditRepository(licitacion, rowVersion: 1);
        var handler = new CerrarLicitacionHandler(repository, Clock);

        await handler.HandleAsync(
            new CerrarLicitacionCommand(licitacion.Id, "Cancelada por lack of budget"));

        Assert.Equal("Cancelada por lack of budget", licitacion.MotivoCierre);
        Assert.Equal(Clock.UtcNow, licitacion.CerradaEn);
    }

    [Fact]
    public async Task Handle_TransicionPublicadaACerrada_RegistraMotivoYFecha()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Clock.UtcNow.AddDays(10), 1_000_000m);
        licitacion.Publicar(Clock.UtcNow);
        var repository = new FakeEditRepository(licitacion, rowVersion: 2);
        var handler = new CerrarLicitacionHandler(repository, Clock);

        await handler.HandleAsync(
            new CerrarLicitacionCommand(licitacion.Id, "Cierre por vencimiento"));

        Assert.Equal("Cierre por vencimiento", licitacion.MotivoCierre);
        Assert.Equal(Clock.UtcNow, licitacion.CerradaEn);
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
