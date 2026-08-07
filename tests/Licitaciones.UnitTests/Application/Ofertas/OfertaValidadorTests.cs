using Licitaciones.Application.Ofertas.Common;
using Licitaciones.Application.Ofertas.Exceptions;
using Licitaciones.Application.Ofertas.Ports;
using Licitaciones.UnitTests.Common;

namespace Licitaciones.UnitTests.Application.Ofertas;

public sealed class OfertaValidadorTests
{
    private static readonly FakeClock Clock =
        new(new(2026, 7, 24, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task ValidarLicitacionAbierta_LicitacionNoExiste_Rechaza()
    {
        var validador = CrearValidador(
            licitacionId: Guid.NewGuid(),
            estaPublicada: false,
            fechaCierre: null);

        var exception = await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => validador.ValidarLicitacionAbiertaAsync(Guid.NewGuid(), "registrar"));

        Assert.Contains("no existe", exception.Message);
    }

    [Fact]
    public async Task ValidarLicitacionAbierta_NoPublicada_Rechaza()
    {
        var licitacionId = Guid.NewGuid();
        var validador = CrearValidador(
            licitacionId: licitacionId,
            estaPublicada: false,
            fechaCierre: Clock.UtcNow.AddDays(10));

        var exception = await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => validador.ValidarLicitacionAbiertaAsync(licitacionId, "registrar"));

        Assert.Contains("licitaciones publicadas", exception.Message);
    }

    [Fact]
    public async Task ValidarLicitacionAbierta_Vencida_Rechaza()
    {
        var licitacionId = Guid.NewGuid();
        var validador = CrearValidador(
            licitacionId: licitacionId,
            estaPublicada: true,
            fechaCierre: Clock.UtcNow.AddDays(-1));

        var exception = await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => validador.ValidarLicitacionAbiertaAsync(licitacionId, "registrar"));

        Assert.Contains("vencidas", exception.Message);
    }

    [Fact]
    public async Task ValidarLicitacionAbierta_FechaDeCierreIgualAhora_Rechaza()
    {
        var licitacionId = Guid.NewGuid();
        var validador = CrearValidador(
            licitacionId: licitacionId,
            estaPublicada: true,
            fechaCierre: Clock.UtcNow);

        var exception = await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => validador.ValidarLicitacionAbiertaAsync(licitacionId, "registrar"));

        Assert.Contains("vencidas", exception.Message);
    }

    [Fact]
    public async Task ValidarLicitacionAbierta_SeVenceAlAvanzarElReloj_Rechaza()
    {
        var licitacionId = Guid.NewGuid();
        var validador = CrearValidador(
            licitacionId: licitacionId,
            estaPublicada: true,
            fechaCierre: Clock.UtcNow.AddDays(5));

        await validador.ValidarLicitacionAbiertaAsync(licitacionId, "registrar");

        Clock.Advance(TimeSpan.FromDays(6));

        await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => validador.ValidarLicitacionAbiertaAsync(licitacionId, "registrar"));
    }

    [Fact]
    public async Task ValidarLicitacionAbierta_PublicadaYNoVencida_Acepta()
    {
        var licitacionId = Guid.NewGuid();
        var validador = CrearValidador(
            licitacionId: licitacionId,
            estaPublicada: true,
            fechaCierre: Clock.UtcNow.AddDays(10));

        await validador.ValidarLicitacionAbiertaAsync(licitacionId, "registrar");
    }

    [Fact]
    public async Task ValidarMonto_MontoNoPositivo_Rechaza()
    {
        var validador = CrearValidador(
            licitacionId: Guid.NewGuid(),
            estaPublicada: true,
            fechaCierre: Clock.UtcNow.AddDays(10),
            presupuesto: 1_000_000m);

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => validador.ValidarMontoAsync(0m, Guid.NewGuid()));

        Assert.Equal("MontoOfertadoCrc", exception.ParamName);
        Assert.Contains("mayor que cero", exception.Message);
    }

    [Fact]
    public async Task ValidarMonto_MontoSuperaPresupuesto_Rechaza()
    {
        var licitacionId = Guid.NewGuid();
        var validador = CrearValidador(
            licitacionId: licitacionId,
            estaPublicada: true,
            fechaCierre: Clock.UtcNow.AddDays(10),
            presupuesto: 1_000_000m);

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => validador.ValidarMontoAsync(1_100_000m, licitacionId));

        Assert.Contains("no puede superar el presupuesto", exception.Message);
    }

    [Theory]
    [InlineData(999_999.99)]
    [InlineData(1_000_000)]
    public async Task ValidarMonto_MontoNoSuperaPresupuesto_Acepta(decimal monto)
    {
        var licitacionId = Guid.NewGuid();
        var validador = CrearValidador(
            licitacionId: licitacionId,
            estaPublicada: true,
            fechaCierre: Clock.UtcNow.AddDays(10),
            presupuesto: 1_000_000m);

        await validador.ValidarMontoAsync(monto, licitacionId);
    }

    private static OfertaValidador CrearValidador(
        Guid licitacionId,
        bool estaPublicada,
        DateTimeOffset? fechaCierre,
        decimal presupuesto = 1_000_000m) =>
        new(
            new FakeValidacionRepository(
                licitacionId,
                estaPublicada,
                fechaCierre,
                presupuesto),
            Clock);

    private sealed class FakeValidacionRepository(
        Guid licitacionId,
        bool estaPublicada,
        DateTimeOffset? fechaCierre,
        decimal presupuesto) : IOfertaValidacionRepository
    {
        public Task<bool> ExisteLicitacionPublicadaAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(id == licitacionId && estaPublicada);

        public Task<DateTimeOffset?> ObtenerFechaCierreAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(id == licitacionId ? fechaCierre : null);

        public Task<decimal> ObtenerPresupuestoAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(id == licitacionId ? presupuesto : 0m);

        public Task<bool> ProveedorExisteAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public Task<bool> YaTieneOfertaAsync(
            Guid licId,
            Guid provId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }
}
