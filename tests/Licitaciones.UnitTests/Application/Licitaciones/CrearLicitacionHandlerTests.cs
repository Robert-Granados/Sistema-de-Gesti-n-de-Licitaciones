using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Licitaciones.Crear;
using Licitaciones.Application.Licitaciones.Exceptions;
using Licitaciones.Application.Licitaciones.Ports;
using Licitaciones.Domain.Entities;
using Licitaciones.Domain.Enums;

namespace Licitaciones.UnitTests.Application.Licitaciones;

public sealed class CrearLicitacionHandlerTests
{
    private static readonly FakeClock Clock = new(new(2026, 7, 24, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Handle_ConDatosValidos_RegistraLicitacion()
    {
        var repository = new FakeLicitacionRepository();
        var handler = new CrearLicitacionHandler(repository, Clock);

        var result = await handler.HandleAsync(
            new CrearLicitacionCommand(
                " LIC-001 ",
                "Equipo de cómputo",
                Clock.UtcNow.AddDays(5),
                1_000_000m));

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("LIC-001", result.Codigo);
        Assert.Single(repository.Agregados);
        Assert.Equal(EstadoLicitacion.Borrador, repository.Agregados[0].Estado);
    }

    [Fact]
    public async Task Handle_ConCodigoDuplicadoNormalizado_Rechaza()
    {
        var repository = new FakeLicitacionRepository(["LIC-001"]);
        var handler = new CrearLicitacionHandler(repository, Clock);

        var exception = await Assert.ThrowsAsync<LicitacionDuplicadaException>(
            () => handler.HandleAsync(
                new CrearLicitacionCommand(
                    "  lic-001  ",
                    "Equipo de cómputo",
                    Clock.UtcNow.AddDays(5),
                    1_000_000m)));

        Assert.Contains("código", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(repository.Agregados);
    }

    [Fact]
    public async Task Handle_ConPresupuestoCero_Rechaza()
    {
        var repository = new FakeLicitacionRepository();
        var handler = new CrearLicitacionHandler(repository, Clock);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => handler.HandleAsync(
                new CrearLicitacionCommand(
                    "LIC-001",
                    "Título",
                    Clock.UtcNow.AddDays(5),
                    0m)));

        Assert.Empty(repository.Agregados);
    }

    [Fact]
    public async Task Handle_ConFechaCierrePasada_Rechaza()
    {
        var repository = new FakeLicitacionRepository();
        var handler = new CrearLicitacionHandler(repository, Clock);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => handler.HandleAsync(
                new CrearLicitacionCommand(
                    "LIC-001",
                    "Título",
                    Clock.UtcNow.AddDays(-1),
                    100m)));

        Assert.Empty(repository.Agregados);
    }

    [Fact]
    public async Task Handle_ConCodigoVariacionDeCasoIgnoraEspacios_RechazaDuplicado()
    {
        var repository = new FakeLicitacionRepository(["LIC-001"]);
        var handler = new CrearLicitacionHandler(repository, Clock);

        var exception = await Assert.ThrowsAsync<LicitacionDuplicadaException>(
            () => handler.HandleAsync(
                new CrearLicitacionCommand(
                    " lic-001 ",
                    "Otra descripción",
                    Clock.UtcNow.AddDays(10),
                    500_000m)));

        Assert.Contains("código", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(repository.Agregados);
    }

    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }

    private sealed class FakeLicitacionRepository(
        IEnumerable<string>? codigosNormalizados = null) : ILicitacionRepository
    {
        private readonly HashSet<string> _codigosNormalizados =
            new(codigosNormalizados ?? [], StringComparer.Ordinal);

        public List<Licitacion> Agregados { get; } = [];

        public Task<bool> ExisteCodigoNormalizadoAsync(
            string codigoNormalizado,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_codigosNormalizados.Contains(codigoNormalizado));

        public Task AgregarAsync(
            Licitacion licitacion,
            CancellationToken cancellationToken = default)
        {
            Agregados.Add(licitacion);
            _codigosNormalizados.Add(licitacion.Codigo.Trim().ToUpperInvariant());
            return Task.CompletedTask;
        }
    }
}
