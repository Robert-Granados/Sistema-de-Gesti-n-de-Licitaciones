using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Licitaciones.Exceptions;
using Licitaciones.Application.Ofertas.Exceptions;
using Licitaciones.Application.Ofertas.Ports;
using Licitaciones.Application.Ofertas.Registrar;
using Licitaciones.Domain.Entities;
using Oferta = Licitaciones.Domain.Entities.Oferta;

namespace Licitaciones.UnitTests.Application.Ofertas;

public sealed class RegistrarOfertaHandlerTests
{
    private static readonly FakeClock Clock = new(new(2026, 7, 24, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Handle_DatosValidos_RegistraOferta()
    {
        var licitacionId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();
        var validacion = new FakeValidacionRepository(
            licitacionId: licitacionId,
            estaPublicada: true,
            fechaCierre: Clock.UtcNow.AddDays(10),
            presupuesto: 1_000_000m,
            proveedorExiste: true,
            yaTieneOferta: false);
        var write = new FakeWriteRepository();
        var handler = new RegistrarOfertaHandler(validacion, write, Clock);

        var result = await handler.HandleAsync(
            new RegistrarOfertaCommand(licitacionId, proveedorId, 500_000m));

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(licitacionId, result.LicitacionId);
        Assert.Equal(proveedorId, result.ProveedorId);
        Assert.True(write.Agregada);
        Assert.Equal(Clock.UtcNow, write.Oferta!.FechaRegistro);
    }

    [Fact]
    public async Task Handle_LicitacionNoExiste_Rechaza()
    {
        var validacion = new FakeValidacionRepository(
            licitacionId: Guid.NewGuid(),
            estaPublicada: false,
            fechaCierre: null,
            presupuesto: 0m,
            proveedorExiste: true,
            yaTieneOferta: false);
        var write = new FakeWriteRepository();
        var handler = new RegistrarOfertaHandler(validacion, write, Clock);

        await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => handler.HandleAsync(
                new RegistrarOfertaCommand(Guid.NewGuid(), Guid.NewGuid(), 100m)));
    }

    [Fact]
    public async Task Handle_LicitacionNoPublicada_Rechaza()
    {
        var licitacionId = Guid.NewGuid();
        var validacion = new FakeValidacionRepository(
            licitacionId: licitacionId,
            estaPublicada: false,
            fechaCierre: Clock.UtcNow.AddDays(10),
            presupuesto: 1_000_000m,
            proveedorExiste: true,
            yaTieneOferta: false);
        var write = new FakeWriteRepository();
        var handler = new RegistrarOfertaHandler(validacion, write, Clock);

        await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => handler.HandleAsync(
                new RegistrarOfertaCommand(licitacionId, Guid.NewGuid(), 100m)));
    }

    [Fact]
    public async Task Handle_LicitacionVencida_Rechaza()
    {
        var licitacionId = Guid.NewGuid();
        var validacion = new FakeValidacionRepository(
            licitacionId: licitacionId,
            estaPublicada: true,
            fechaCierre: Clock.UtcNow.AddDays(-1),
            presupuesto: 1_000_000m,
            proveedorExiste: true,
            yaTieneOferta: false);
        var write = new FakeWriteRepository();
        var handler = new RegistrarOfertaHandler(validacion, write, Clock);

        await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => handler.HandleAsync(
                new RegistrarOfertaCommand(licitacionId, Guid.NewGuid(), 100m)));
    }

    [Fact]
    public async Task Handle_ProveedorNoExiste_Rechaza()
    {
        var licitacionId = Guid.NewGuid();
        var validacion = new FakeValidacionRepository(
            licitacionId: licitacionId,
            estaPublicada: true,
            fechaCierre: Clock.UtcNow.AddDays(10),
            presupuesto: 1_000_000m,
            proveedorExiste: false,
            yaTieneOferta: false);
        var write = new FakeWriteRepository();
        var handler = new RegistrarOfertaHandler(validacion, write, Clock);

        await Assert.ThrowsAsync<ProveedorNoEncontradoException>(
            () => handler.HandleAsync(
                new RegistrarOfertaCommand(licitacionId, Guid.NewGuid(), 100m)));
    }

    [Theory]
    [InlineData(900_000, 1_000_000, true)]
    [InlineData(1_000_000, 1_000_000, true)]
    [InlineData(1_100_000, 1_000_000, false)]
    public async Task Handle_MontoVsPresupuesto_ValidaTopePresupuestario(
        int montoOfertado,
        int presupuesto,
        bool seEsperaAceptacion)
    {
        var licitacionId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();
        var validacion = new FakeValidacionRepository(
            licitacionId: licitacionId,
            estaPublicada: true,
            fechaCierre: Clock.UtcNow.AddDays(10),
            presupuesto: (decimal)presupuesto,
            proveedorExiste: true,
            yaTieneOferta: false);
        var write = new FakeWriteRepository();
        var handler = new RegistrarOfertaHandler(validacion, write, Clock);

        if (seEsperaAceptacion)
        {
            var result = await handler.HandleAsync(
                new RegistrarOfertaCommand(licitacionId, proveedorId, montoOfertado));

            Assert.True(write.Agregada);
            Assert.Equal((decimal)montoOfertado, write.Oferta!.MontoOfertadoCrc);
        }
        else
        {
            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => handler.HandleAsync(
                    new RegistrarOfertaCommand(licitacionId, proveedorId, montoOfertado)));

            Assert.Equal(nameof(RegistrarOfertaCommand.MontoOfertadoCrc), exception.ParamName);
            Assert.Contains("no puede superar el presupuesto", exception.Message);
            Assert.False(write.Agregada);
        }
    }

    [Fact]
    public async Task Handle_MontoCero_Rechaza()
    {
        var licitacionId = Guid.NewGuid();
        var validacion = new FakeValidacionRepository(
            licitacionId: licitacionId,
            estaPublicada: true,
            fechaCierre: Clock.UtcNow.AddDays(10),
            presupuesto: 1_000_000m,
            proveedorExiste: true,
            yaTieneOferta: false);
        var write = new FakeWriteRepository();
        var handler = new RegistrarOfertaHandler(validacion, write, Clock);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => handler.HandleAsync(
                new RegistrarOfertaCommand(licitacionId, Guid.NewGuid(), 0m)));
    }

    [Fact]
    public async Task Handle_YaTieneOferta_Rechaza()
    {
        var licitacionId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();
        var validacion = new FakeValidacionRepository(
            licitacionId: licitacionId,
            estaPublicada: true,
            fechaCierre: Clock.UtcNow.AddDays(10),
            presupuesto: 1_000_000m,
            proveedorExiste: true,
            yaTieneOferta: true);
        var write = new FakeWriteRepository();
        var handler = new RegistrarOfertaHandler(validacion, write, Clock);

        await Assert.ThrowsAsync<OfertaDuplicadaException>(
            () => handler.HandleAsync(
                new RegistrarOfertaCommand(licitacionId, proveedorId, 500_000m)));
    }

    [Fact]
    public async Task Handle_SegundaOfertaMismoPar_RechazaConMensajeClaroSinAfectarOriginal()
    {
        var licitacionId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();
        var ofertasRegistradas = new List<Oferta>();
        var validacion = new FakeValidacionRepository(
            licitacionId: licitacionId,
            estaPublicada: true,
            fechaCierre: Clock.UtcNow.AddDays(10),
            presupuesto: 1_000_000m,
            proveedorExiste: true,
            yaTieneOferta: false,
            ofertasRegistradas: ofertasRegistradas);
        var write = new FakeWriteRepository(ofertasRegistradas);
        var handler = new RegistrarOfertaHandler(validacion, write, Clock);

        var primera = await handler.HandleAsync(
            new RegistrarOfertaCommand(licitacionId, proveedorId, 500_000m));

        Assert.Single(ofertasRegistradas);

        var exception = await Assert.ThrowsAsync<OfertaDuplicadaException>(
            () => handler.HandleAsync(
                new RegistrarOfertaCommand(licitacionId, proveedorId, 400_000m)));

        Assert.Equal(
            "Este proveedor ya tiene una oferta registrada para esta licitación.",
            exception.Message);
        Assert.Single(ofertasRegistradas);
        Assert.Equal(500_000m, ofertasRegistradas[0].MontoOfertadoCrc);
        Assert.Equal(primera.Id, ofertasRegistradas[0].Id);
    }

    [Fact]
    public async Task Handle_LicitacionIdVacio_Rechaza()
    {
        var validacion = new FakeValidacionRepository(
            licitacionId: Guid.NewGuid(),
            estaPublicada: true,
            fechaCierre: Clock.UtcNow.AddDays(10),
            presupuesto: 1_000_000m,
            proveedorExiste: true,
            yaTieneOferta: false);
        var write = new FakeWriteRepository();
        var handler = new RegistrarOfertaHandler(validacion, write, Clock);

        await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => handler.HandleAsync(
                new RegistrarOfertaCommand(Guid.Empty, Guid.NewGuid(), 100m)));
    }

    [Fact]
    public async Task Handle_ProveedorIdVacio_Rechaza()
    {
        var licitacionId = Guid.NewGuid();
        var validacion = new FakeValidacionRepository(
            licitacionId: licitacionId,
            estaPublicada: true,
            fechaCierre: Clock.UtcNow.AddDays(10),
            presupuesto: 1_000_000m,
            proveedorExiste: true,
            yaTieneOferta: false);
        var write = new FakeWriteRepository();
        var handler = new RegistrarOfertaHandler(validacion, write, Clock);

        await Assert.ThrowsAsync<ProveedorNoEncontradoException>(
            () => handler.HandleAsync(
                new RegistrarOfertaCommand(licitacionId, Guid.Empty, 100m)));
    }

    [Fact]
    public async Task Handle_DatosInvalidos_ValidaExistenciaAntesDelMonto()
    {
        var licitacionId = Guid.NewGuid();
        var validacion = new FakeValidacionRepository(
            licitacionId: licitacionId,
            estaPublicada: true,
            fechaCierre: Clock.UtcNow.AddDays(10),
            presupuesto: 1_000_000m,
            proveedorExiste: false,
            yaTieneOferta: false);
        var handler = new RegistrarOfertaHandler(
            validacion,
            new FakeWriteRepository(),
            Clock);

        await Assert.ThrowsAsync<ProveedorNoEncontradoException>(
            () => handler.HandleAsync(
                new RegistrarOfertaCommand(licitacionId, Guid.NewGuid(), 0m)));
    }

    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }

    private sealed class FakeValidacionRepository(
        Guid licitacionId,
        bool estaPublicada,
        DateTimeOffset? fechaCierre,
        decimal presupuesto,
        bool proveedorExiste,
        bool yaTieneOferta,
        IReadOnlyCollection<Oferta>? ofertasRegistradas = null) : IOfertaValidacionRepository
    {
        public Task<bool> ExisteLicitacionPublicadaAsync(
            Guid id, CancellationToken ct = default) =>
            Task.FromResult(id == licitacionId && estaPublicada);

        public Task<DateTimeOffset?> ObtenerFechaCierreAsync(
            Guid id, CancellationToken ct = default) =>
            Task.FromResult(id == licitacionId ? fechaCierre : null);

        public Task<decimal> ObtenerPresupuestoAsync(
            Guid id, CancellationToken ct = default) =>
            Task.FromResult(id == licitacionId ? presupuesto : 0m);

        public Task<bool> ProveedorExisteAsync(
            Guid id, CancellationToken ct = default) =>
            Task.FromResult(id != Guid.Empty && proveedorExiste);

        public Task<bool> YaTieneOfertaAsync(
            Guid licId, Guid provId, CancellationToken ct = default) =>
            Task.FromResult(
                yaTieneOferta
                || ofertasRegistradas?.Any(
                    oferta => oferta.LicitacionId == licId
                        && oferta.ProveedorId == provId) == true);
    }

    private sealed class FakeWriteRepository(
        List<Oferta>? ofertasRegistradas = null) : IOfertaWriteRepository
    {
        public bool Agregada { get; private set; }
        public Oferta? Oferta { get; private set; }

        public Task AgregarAsync(
            Oferta oferta,
            CancellationToken cancellationToken = default)
        {
            Agregada = true;
            Oferta = oferta;
            ofertasRegistradas?.Add(oferta);
            return Task.CompletedTask;
        }
    }
}
