using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Ofertas.Common;
using Licitaciones.Application.Ofertas.Editar;
using Licitaciones.Application.Ofertas.Exceptions;
using Licitaciones.Application.Ofertas.Listar;
using Licitaciones.Application.Ofertas.OpcionesFiltro;
using Licitaciones.Application.Ofertas.Ports;
using Licitaciones.Domain.Entities;
using Oferta = Licitaciones.Domain.Entities.Oferta;

namespace Licitaciones.UnitTests.Application.Ofertas;

public sealed class EditarOfertaHandlerTests
{
    private static readonly DateTimeOffset Ahora =
        new(2026, 7, 24, 12, 0, 0, TimeSpan.Zero);

    private static readonly FakeClock Clock = new(Ahora);

    [Fact]
    public async Task Obtener_ConIdValido_DevuelveOfertaParaEdicion()
    {
        var oferta = CrearOferta();
        var write = new FakeWriteRepository([oferta]);
        var handler = new EditarOfertaHandler(
            new FakeReadRepository([oferta]),
            write,
            new OfertaValidador(CrearValidacion(fechaCierre: Ahora.AddDays(10)), Clock));

        var dto = await handler.ObtenerAsync(oferta.Id);

        Assert.NotNull(dto);
        Assert.Equal(oferta.Id, dto.Id);
        Assert.Equal("LIC-001", dto.CodigoLicitacion);
        Assert.Equal("Proveedor A", dto.NombreProveedor);
        Assert.Equal(oferta.MontoOfertadoCrc, dto.MontoOfertadoCrc);
    }

    [Fact]
    public async Task Obtener_ConIdVacio_DevuelveNull()
    {
        var handler = new EditarOfertaHandler(
            new FakeReadRepository(),
            new FakeWriteRepository(),
            new OfertaValidador(CrearValidacion(), Clock));

        Assert.Null(await handler.ObtenerAsync(Guid.Empty));
    }

    [Fact]
    public async Task Obtener_OfertaNoExiste_DevuelveNull()
    {
        var handler = new EditarOfertaHandler(
            new FakeReadRepository(),
            new FakeWriteRepository(),
            new OfertaValidador(CrearValidacion(), Clock));

        Assert.Null(await handler.ObtenerAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Handle_DatosValidos_ActualizaElMonto()
    {
        var oferta = CrearOferta();
        var write = new FakeWriteRepository([oferta]);
        var handler = new EditarOfertaHandler(
            new FakeReadRepository([oferta]),
            write,
            new OfertaValidador(CrearValidacion(fechaCierre: Ahora.AddDays(10)), Clock));

        await handler.HandleAsync(new EditarOfertaCommand(oferta.Id, 750_000m));

        Assert.Equal(750_000m, oferta.MontoOfertadoCrc);
        Assert.Single(write.Actualizadas);
        Assert.Equal(oferta.Id, write.Actualizadas[0].Id);
    }

    [Fact]
    public async Task Handle_OfertaNoExiste_Rechaza()
    {
        var handler = new EditarOfertaHandler(
            new FakeReadRepository(),
            new FakeWriteRepository(),
            new OfertaValidador(CrearValidacion(), Clock));

        await Assert.ThrowsAsync<OfertaNoEncontradaException>(
            () => handler.HandleAsync(new EditarOfertaCommand(Guid.NewGuid(), 100m)));
    }

    [Fact]
    public async Task Handle_LicitacionCerrada_RechazaSinActualizar()
    {
        var oferta = CrearOferta();
        var write = new FakeWriteRepository([oferta]);
        var handler = new EditarOfertaHandler(
            new FakeReadRepository([oferta]),
            write,
            new OfertaValidador(CrearValidacion(estaPublicada: false), Clock));

        var exception = await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => handler.HandleAsync(new EditarOfertaCommand(oferta.Id, 750_000m)));

        Assert.Equal(
            "Solo se pueden editar ofertas para licitaciones publicadas.",
            exception.Message);
        Assert.Equal(500_000m, oferta.MontoOfertadoCrc);
        Assert.Empty(write.Actualizadas);
    }

    [Fact]
    public async Task Handle_LicitacionVencida_RechazaSinActualizar()
    {
        var oferta = CrearOferta();
        var write = new FakeWriteRepository([oferta]);
        var handler = new EditarOfertaHandler(
            new FakeReadRepository([oferta]),
            write,
            new OfertaValidador(CrearValidacion(fechaCierre: Ahora.AddDays(-1)), Clock));

        var exception = await Assert.ThrowsAsync<LicitacionNoDisponibleException>(
            () => handler.HandleAsync(new EditarOfertaCommand(oferta.Id, 750_000m)));

        Assert.Equal(
            "No se pueden editar ofertas para licitaciones vencidas.",
            exception.Message);
        Assert.Equal(500_000m, oferta.MontoOfertadoCrc);
        Assert.Empty(write.Actualizadas);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_MontoNoPositivo_RechazaSinActualizar(decimal monto)
    {
        var oferta = CrearOferta();
        var write = new FakeWriteRepository([oferta]);
        var handler = new EditarOfertaHandler(
            new FakeReadRepository([oferta]),
            write,
            new OfertaValidador(CrearValidacion(fechaCierre: Ahora.AddDays(10)), Clock));

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => handler.HandleAsync(new EditarOfertaCommand(oferta.Id, monto)));

        Assert.Equal("MontoOfertadoCrc", exception.ParamName);
        Assert.Equal(500_000m, oferta.MontoOfertadoCrc);
        Assert.Empty(write.Actualizadas);
    }

    [Fact]
    public async Task Handle_MontoExcedePresupuesto_RechazaSinActualizar()
    {
        var oferta = CrearOferta();
        var write = new FakeWriteRepository([oferta]);
        var handler = new EditarOfertaHandler(
            new FakeReadRepository([oferta]),
            write,
            new OfertaValidador(
                CrearValidacion(presupuesto: 600_000m),
                Clock));

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => handler.HandleAsync(new EditarOfertaCommand(oferta.Id, 750_000m)));

        Assert.Contains("no puede superar el presupuesto", exception.Message);
        Assert.Equal(500_000m, oferta.MontoOfertadoCrc);
        Assert.Empty(write.Actualizadas);
    }

    [Fact]
    public async Task Handle_MontoIgualAlPresupuesto_Actualiza()
    {
        var oferta = CrearOferta();
        var write = new FakeWriteRepository([oferta]);
        var handler = new EditarOfertaHandler(
            new FakeReadRepository([oferta]),
            write,
            new OfertaValidador(
                CrearValidacion(presupuesto: 750_000m),
                Clock));

        await handler.HandleAsync(new EditarOfertaCommand(oferta.Id, 750_000m));

        Assert.Equal(750_000m, oferta.MontoOfertadoCrc);
        Assert.Single(write.Actualizadas);
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
        public List<Oferta> Actualizadas { get; } = [];

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
            CancellationToken cancellationToken = default)
        {
            Actualizadas.Add(oferta);
            return Task.CompletedTask;
        }

        public Task EliminarAsync(
            Oferta oferta,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeReadRepository(
        IReadOnlyCollection<Oferta>? ofertas = null) : IOfertaReadRepository
    {
        public Task<PaginaResultado<OfertaListadoDto>> ListarAsync(
            OfertasConsulta consulta,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new PaginaResultado<OfertaListadoDto>(
                [],
                totalRegistros: 0,
                paginaActual: consulta.Page,
                tamanoPagina: consulta.PageSize));

        public Task<OpcionesFiltroOfertasDto> ObtenerOpcionesFiltroAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcionesFiltroOfertasDto([], []));

        public Task<EditarOfertaDto?> ObtenerParaEdicionAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var oferta = ofertas?.FirstOrDefault(oferta => oferta.Id == id);

            if (oferta is null)
            {
                return Task.FromResult<EditarOfertaDto?>(null);
            }

            return Task.FromResult<EditarOfertaDto?>(
                new EditarOfertaDto(
                    oferta.Id,
                    oferta.LicitacionId,
                    "LIC-001",
                    "Proveedor A",
                    oferta.MontoOfertadoCrc));
        }
    }
}
