using Licitaciones.Application.Licitaciones.Eliminar;
using Licitaciones.Application.Licitaciones.Exceptions;
using Licitaciones.Application.Licitaciones.Ports;
using Licitaciones.Domain.Entities;
using Licitaciones.UnitTests.Common;

namespace Licitaciones.UnitTests.Application.Licitaciones;

public sealed class EliminarLicitacionHandlerTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_LicitacionConOfertas_AplicaBorradoLogico()
    {
        var licitacion = new Licitacion("LIC-001", "Título", DateTimeOffset.UtcNow.AddDays(10), 1_000_000m);
        var repository = new FakeDeleteRepository(licitacion, tieneOfertas: true);
        var handler = new EliminarLicitacionHandler(repository, new FakeClock(Now));

        var resultado = await handler.HandleAsync(
            new EliminarLicitacionCommand(licitacion.Id));

        Assert.True(resultado.TeniaOfertas);
        Assert.True(licitacion.EstaEliminada);
        Assert.Equal(Now, licitacion.EliminadoEn);
        Assert.True(repository.Guardado);
    }

    [Fact]
    public async Task Handle_LicitacionSinOfertas_AplicaMismoBorradoLogico()
    {
        var licitacion = new Licitacion("LIC-001", "Título", DateTimeOffset.UtcNow.AddDays(10), 1_000_000m);
        var repository = new FakeDeleteRepository(licitacion, tieneOfertas: false);
        var handler = new EliminarLicitacionHandler(repository, new FakeClock(Now));

        var resultado = await handler.HandleAsync(
            new EliminarLicitacionCommand(licitacion.Id));

        Assert.False(resultado.TeniaOfertas);
        Assert.True(licitacion.EstaEliminada);
        Assert.True(repository.Guardado);
    }

    [Fact]
    public async Task Handle_LicitacionInexistente_LanzaExcepcion()
    {
        var handler = new EliminarLicitacionHandler(
            new FakeDeleteRepository(null, tieneOfertas: false),
            new FakeClock(Now));

        await Assert.ThrowsAsync<LicitacionNoEncontradaException>(
            () => handler.HandleAsync(
                new EliminarLicitacionCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task Handle_IdVacio_LanzaExcepcion()
    {
        var handler = new EliminarLicitacionHandler(
            new FakeDeleteRepository(null, tieneOfertas: false),
            new FakeClock(Now));

        await Assert.ThrowsAsync<LicitacionNoEncontradaException>(
            () => handler.HandleAsync(
                new EliminarLicitacionCommand(Guid.Empty)));
    }

    [Fact]
    public async Task Handle_LicitacionYaEliminada_LanzaExcepcion()
    {
        var licitacion = new Licitacion("LIC-001", "Título", DateTimeOffset.UtcNow.AddDays(10), 1_000_000m);
        licitacion.Eliminar(DateTimeOffset.UtcNow);
        var repository = new FakeDeleteRepository(licitacion, tieneOfertas: false);
        var handler = new EliminarLicitacionHandler(repository, new FakeClock(Now));

        await Assert.ThrowsAsync<LicitacionNoEncontradaException>(
            () => handler.HandleAsync(
                new EliminarLicitacionCommand(licitacion.Id)));
    }

    private sealed class FakeDeleteRepository(
        Licitacion? licitacion,
        bool tieneOfertas) : ILicitacionDeleteRepository
    {
        public bool Guardado { get; private set; }

        public Task<Licitacion?> ObtenerActivaPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(licitacion?.Id == id && !licitacion.EstaEliminada
                ? licitacion : null);

        public Task<bool> TieneOfertasAsync(
            Guid licitacionId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(tieneOfertas);

        public Task GuardarBorradoLogicoAsync(
            Licitacion licitacionAEliminar,
            CancellationToken cancellationToken = default)
        {
            Guardado = true;
            return Task.CompletedTask;
        }
    }
}
