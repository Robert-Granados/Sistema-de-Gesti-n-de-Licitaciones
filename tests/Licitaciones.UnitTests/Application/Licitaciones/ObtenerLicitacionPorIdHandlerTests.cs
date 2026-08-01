using Licitaciones.Application.Licitaciones.Detalle;
using Licitaciones.Application.Licitaciones.Ports;
using Licitaciones.Domain.Entities;
using Licitaciones.Domain.Enums;

namespace Licitaciones.UnitTests.Application.Licitaciones;

public sealed class ObtenerLicitacionPorIdHandlerTests
{
    [Fact]
    public async Task Handle_SinOfertas_MejorOfertaEsNula()
    {
        var licitacion = new Licitacion("LIC-001", "Título", DateTimeOffset.UtcNow.AddDays(5), 1_000_000m);
        var repository = new FakeDetalleRepository(licitacion, [], NivelesDefault(), null);
        var handler = new ObtenerLicitacionPorIdHandler(repository);

        var result = await handler.HandleAsync(
            new ObtenerLicitacionPorIdQuery(licitacion.Id));

        Assert.NotNull(result);
        Assert.Null(result.MejorOferta);
        Assert.Empty(result.Ofertas.Elementos);
    }

    [Fact]
    public async Task Handle_ConUnaOferta_MejorOfertaEsEsa()
    {
        var licitacion = new Licitacion("LIC-001", "Título", DateTimeOffset.UtcNow.AddDays(5), 1_000_000m);
        var proveedor = new Proveedor("Proveedor A");
        var ofertas = new List<OfertaBasica>
        {
            new(Guid.NewGuid(), proveedor.Id, proveedor.Nombre, 500_000m, DateTimeOffset.UtcNow)
        };
        var repository = new FakeDetalleRepository(licitacion, ofertas, NivelesDefault(), null);
        var handler = new ObtenerLicitacionPorIdHandler(repository);

        var result = await handler.HandleAsync(
            new ObtenerLicitacionPorIdQuery(licitacion.Id));

        Assert.NotNull(result);
        Assert.NotNull(result!.MejorOferta);
        Assert.Equal(500_000m, result.MejorOferta.MontoOfertadoCrc);
        Assert.Equal(ClasificacionAhorro.OfertaConveniente, result.MejorOferta.Clasificacion);
    }

    [Fact]
    public async Task Handle_ConEmpate_MenorMontoPrimeroRegistrado()
    {
        var licitacion = new Licitacion("LIC-001", "Título", DateTimeOffset.UtcNow.AddDays(5), 1_000_000m);
        var fechaTardia = new DateTimeOffset(2026, 7, 25, 12, 0, 0, TimeSpan.Zero);
        var fechaTemprana = new DateTimeOffset(2026, 7, 24, 10, 0, 0, TimeSpan.Zero);
        var ofertas = new List<OfertaBasica>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Proveedor B", 300_000m, fechaTardia),
            new(Guid.NewGuid(), Guid.NewGuid(), "Proveedor A", 300_000m, fechaTemprana),
        };
        var repository = new FakeDetalleRepository(licitacion, ofertas, NivelesDefault(), null);
        var handler = new ObtenerLicitacionPorIdHandler(repository);

        var result = await handler.HandleAsync(
            new ObtenerLicitacionPorIdQuery(licitacion.Id));

        Assert.NotNull(result);
        Assert.NotNull(result!.MejorOferta);
        Assert.Equal("Proveedor A", result.MejorOferta.NombreProveedor);
        Assert.Equal(fechaTemprana, result.Ofertas.Elementos[0].FechaRegistro);
    }

    [Fact]
    public async Task Handle_ConMontoExactoPresupuesto_ClasificacionValidaSinAhorro()
    {
        var presupuesto = 1_000_000m;
        var licitacion = new Licitacion("LIC-001", "Título", DateTimeOffset.UtcNow.AddDays(5), presupuesto);
        var ofertas = new List<OfertaBasica>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Proveedor A", presupuesto, DateTimeOffset.UtcNow)
        };
        var repository = new FakeDetalleRepository(licitacion, ofertas, NivelesDefault(), null);
        var handler = new ObtenerLicitacionPorIdHandler(repository);

        var result = await handler.HandleAsync(
            new ObtenerLicitacionPorIdQuery(licitacion.Id));

        Assert.NotNull(result);
        Assert.NotNull(result!.MejorOferta);
        Assert.Equal(ClasificacionAhorro.OfertaValidaSinAhorro, result.MejorOferta.Clasificacion);
    }

    [Fact]
    public async Task Handle_ConAhorroMenor10Porciento_ClasificacionAceptable()
    {
        var presupuesto = 1_000_000m;
        var licitacion = new Licitacion("LIC-001", "Título", DateTimeOffset.UtcNow.AddDays(5), presupuesto);
        var ofertas = new List<OfertaBasica>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Proveedor A", 950_000m, DateTimeOffset.UtcNow)
        };
        var repository = new FakeDetalleRepository(licitacion, ofertas, NivelesDefault(), null);
        var handler = new ObtenerLicitacionPorIdHandler(repository);

        var result = await handler.HandleAsync(
            new ObtenerLicitacionPorIdQuery(licitacion.Id));

        Assert.NotNull(result);
        Assert.NotNull(result!.MejorOferta);
        Assert.Equal(ClasificacionAhorro.OfertaAceptable, result.MejorOferta.Clasificacion);
    }

    [Fact]
    public async Task Handle_ConTipoCambioActivo_MuestraConversion()
    {
        var licitacion = new Licitacion("LIC-001", "Título", DateTimeOffset.UtcNow.AddDays(5), 1_000_000m);
        var ofertas = new List<OfertaBasica>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Proveedor A", 520_000m, DateTimeOffset.UtcNow)
        };
        var tipoCambio = new TipoCambio(520m, DateTimeOffset.UtcNow, activo: true);
        var repository = new FakeDetalleRepository(licitacion, ofertas, NivelesDefault(), tipoCambio);
        var handler = new ObtenerLicitacionPorIdHandler(repository);

        var result = await handler.HandleAsync(
            new ObtenerLicitacionPorIdQuery(licitacion.Id));

        Assert.NotNull(result);
        Assert.NotNull(result!.MejorOferta);
        Assert.NotNull(result.MejorOferta.MontoUsd);
        Assert.Equal(1000m, result.MejorOferta.MontoUsd);
        Assert.NotNull(result.MejorOferta.FechaVigenciaTipoCambio);
    }

    [Fact]
    public async Task Handle_IdVacio_RetornaNull()
    {
        var repository = new FakeDetalleRepository(null, [], [], null);
        var handler = new ObtenerLicitacionPorIdHandler(repository);

        var result = await handler.HandleAsync(
            new ObtenerLicitacionPorIdQuery(Guid.Empty));

        Assert.Null(result);
    }

    [Fact]
    public void ResolverAprobador_ConMontos_SeleccionaNivelCorrecto()
    {
        var niveles = NivelesDefault();

        var a1 = ResolverAprobadorService.Resolver(niveles, 500_000m);
        var a2 = ResolverAprobadorService.Resolver(niveles, 5_000_000m);
        var a3 = ResolverAprobadorService.Resolver(niveles, 50_000_000m);

        Assert.Equal("Encargado de área", a1);
        Assert.Equal("Gerencia", a2);
        Assert.Equal("Junta Directiva", a3);
    }

    [Fact]
    public void ResolverAprobador_MontoFueraDeRango_RetornaNull()
    {
        var niveles = NivelesDefault();
        var aprobador = ResolverAprobadorService.Resolver(niveles, -1m);
        Assert.Null(aprobador);
    }

    private static IReadOnlyList<NivelAprobacion> NivelesDefault()
    {
        return
        [
            new NivelAprobacion(0.01m, 999_999.99m, "Encargado de área"),
            new NivelAprobacion(1_000_000m, 9_999_999.99m, "Gerencia"),
            new NivelAprobacion(10_000_000m, null, "Junta Directiva"),
        ];
    }

    private sealed class FakeDetalleRepository(
        Licitacion? licitacion,
        IReadOnlyList<OfertaBasica> ofertas,
        IReadOnlyList<NivelAprobacion> niveles,
        TipoCambio? tipoCambio) : ILicitacionDetalleRepository
    {
        public Task<LicitacionDetalleCompleta?> ObtenerPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (licitacion is null || licitacion.Id != id)
            {
                return Task.FromResult<LicitacionDetalleCompleta?>(null);
            }

            return Task.FromResult<LicitacionDetalleCompleta?>(
                    new LicitacionDetalleCompleta(
                        licitacion, ofertas, niveles, tipoCambio, []));
        }
    }
}
