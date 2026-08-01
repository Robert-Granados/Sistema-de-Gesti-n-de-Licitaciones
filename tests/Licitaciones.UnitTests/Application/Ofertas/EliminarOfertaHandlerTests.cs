using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Ofertas.Common;
using Licitaciones.Application.Ofertas.Eliminar;
using Licitaciones.Application.Ofertas.Exceptions;
using Licitaciones.Application.Ofertas.Ports;
using Licitaciones.Domain.Entities;
using Oferta = Licitaciones.Domain.Entities.Oferta;

namespace Licitaciones.UnitTests.Application.Ofertas;

public sealed class EliminarOfertaHandlerTests
{
    private static readonly DateTimeOffset Ahora =
        new(2026, 7, 24, 12, 0, 0, TimeSpan.Zero);

    private static readonly FakeClock Clock = new(Ahora);

    [Fact]
    public async Task Handle_LicitacionAbierta_EliminaLaOferta()
    {
        var oferta = CrearOferta();
        var write = new FakeWriteRepository([oferta]);
        var handler = new EliminarOfertaHandler(
            write,
            new OfertaValidador(CrearValidacion(fechaCierre: Ahora.AddDays(10)), Clock));

        var result = await handler.HandleAsync(new EliminarOfertaCommand(oferta.Id));

        Assert.Equal(oferta.Id, result.Id);
        Assert.Contains(oferta.Id, write.Eliminadas);
    }

    [Fact]
    public async Task Handle_OfertaNoExiste_Rechaza()
    {
        var handler = new EliminarOfertaHandler(
            new FakeWriteRepository(),
            new OfertaValidador(CrearValidacion(), Clock));

        await Assert.ThrowsAsync<OfertaNoEncontradaException>(
            () => handler.HandleAsync(new EliminarOfertaCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task Handle_LicitacionCerrada_RechazaSinEliminar()
    {
        var oferta = CrearOferta();
        var write = new FakeWriteRepository([oferta]);
        var handler = new EliminarOfertaHandler(
            write,
            new OfertaValidador(CrearValidacion(estaPublicada: false), Clock));

        var exception = await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => handler.HandleAsync(new EliminarOfertaCommand(oferta.Id)));

        Assert.Equal(
            "Solo se pueden eliminar ofertas para licitaciones publicadas.",
            exception.Message);
        Assert.Empty(write.Eliminadas);
    }

    [Fact]
    public async Task Handle_LicitacionVencida_RechazaSinEliminar()
    {
        var oferta = CrearOferta();
        var write = new FakeWriteRepository([oferta]);
        var handler = new EliminarOfertaHandler(
            write,
            new OfertaValidador(CrearValidacion(fechaCierre: Ahora.AddDays(-1)), Clock));

        var exception = await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => handler.HandleAsync(new EliminarOfertaCommand(oferta.Id)));

        Assert.Equal(
            "No se pueden eliminar ofertas para licitaciones vencidas.",
            exception.Message);
        Assert.Empty(write.Eliminadas);
    }

    [Fact]
    public async Task Handle_FechaCierreIgualAhora_RechazaSinEliminar()
    {
        var oferta = CrearOferta();
        var write = new FakeWriteRepository([oferta]);
        var handler = new EliminarOfertaHandler(
            write,
            new OfertaValidador(CrearValidacion(fechaCierre: Ahora), Clock));

        await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => handler.HandleAsync(new EliminarOfertaCommand(oferta.Id)));

        Assert.Empty(write.Eliminadas);
    }

    private static Oferta CrearOferta() =>
        new(Guid.NewGuid(), Guid.NewGuid(), 500_000m, Ahora);

    private static FakeValidacionRepository CrearValidacion(
        bool estaPublicada = true,
        DateTimeOffset? fechaCierre = null,
        decimal presupuesto = 1_000_000m) =>
        new(estaPublicada, fechaCierre ?? Ahora.AddDays(10), presupuesto);

    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }

    private sealed class FakeValidacionRepository(
        bool estaPublicada,
        DateTimeOffset fechaCierre,
        decimal presupuesto) : IOfertaValidacionRepository
    {
        public Task<bool> ExisteLicitacionPublicadaAsync(
            Guid id, CancellationToken ct = default) =>
            Task.FromResult(estaPublicada);

        public Task<DateTimeOffset?> ObtenerFechaCierreAsync(
            Guid id, CancellationToken ct = default) =>
            Task.FromResult<DateTimeOffset?>(fechaCierre);

        public Task<decimal> ObtenerPresupuestoAsync(
            Guid id, CancellationToken ct = default) =>
            Task.FromResult(presupuesto);

        public Task<bool> ProveedorExisteAsync(
            Guid id, CancellationToken ct = default) =>
            Task.FromResult(true);

        public Task<bool> YaTieneOfertaAsync(
            Guid licitacionId, Guid proveedorId, CancellationToken ct = default) =>
            Task.FromResult(false);
    }

    private sealed class FakeWriteRepository(
        List<Oferta>? ofertas = null) : IOfertaWriteRepository
    {
        public List<Guid> Eliminadas { get; } = [];

        public Task AgregarAsync(
            Oferta oferta,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<Oferta?> ObtenerPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                ofertas?.FirstOrDefault(oferta => oferta.Id == id));

        public Task ActualizarAsync(
            Oferta oferta,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task EliminarAsync(
            Oferta oferta,
            CancellationToken cancellationToken = default)
        {
            ofertas?.Remove(oferta);
            Eliminadas.Add(oferta.Id);
            return Task.CompletedTask;
        }
    }
}
