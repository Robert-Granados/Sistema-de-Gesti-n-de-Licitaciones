using Licitaciones.Application.Licitaciones.Detalle;
using Licitaciones.Application.Licitaciones.Ports;

namespace Licitaciones.UnitTests.Application.Licitaciones;

public sealed class MejorOfertaTests
{
    [Fact]
    public void Calcular_SinOfertas_DevuelveNull()
    {
        var resultado = CalculadorMejorOferta.Calcular([]);

        Assert.Null(resultado);
    }

    [Fact]
    public void Calcular_UnaSolaOferta_EsLaMejor()
    {
        var unica = Oferta(Guid.NewGuid(), "Proveedor Único", 900m, Fecha(10));

        var resultado = CalculadorMejorOferta.Calcular([unica]);

        Assert.NotNull(resultado);
        Assert.Equal("Proveedor Único", resultado.NombreProveedor);
        Assert.Equal(900m, resultado.MontoOfertadoCrc);
    }

    [Fact]
    public void Calcular_ConVariasOfertas_DevuelveLaDeMenorMonto()
    {
        var ofertas = new List<OfertaBasica>
        {
            Oferta(Guid.NewGuid(), "Proveedor B", 1_200m, Fecha(10)),
            Oferta(Guid.NewGuid(), "Proveedor A", 800m, Fecha(9)),
            Oferta(Guid.NewGuid(), "Proveedor C", 1_000m, Fecha(11))
        };

        var resultado = CalculadorMejorOferta.Calcular(ofertas);

        Assert.NotNull(resultado);
        Assert.Equal("Proveedor A", resultado.NombreProveedor);
        Assert.Equal(800m, resultado.MontoOfertadoCrc);
    }

    [Fact]
    public void Calcular_EmpateEnMonto_DesempataPorFechaDeRegistroMasAntigua()
    {
        var ofertas = new List<OfertaBasica>
        {
            Oferta(Guid.NewGuid(), "Proveedor B", 800m, Fecha(10)),
            Oferta(Guid.NewGuid(), "Proveedor A", 800m, Fecha(9))
        };

        var resultado = CalculadorMejorOferta.Calcular(ofertas);

        Assert.NotNull(resultado);
        Assert.Equal("Proveedor A", resultado.NombreProveedor);
        Assert.Equal(Fecha(9), resultado.FechaRegistro);
    }

    [Fact]
    public void Calcular_EmpateEnMontoYFecha_DevuelveLaPrimeraRegistrada()
    {
        var ofertas = new List<OfertaBasica>
        {
            Oferta(Guid.NewGuid(), "Proveedor A", 800m, Fecha(9)),
            Oferta(Guid.NewGuid(), "Proveedor B", 800m, Fecha(9))
        };

        var resultado = CalculadorMejorOferta.Calcular(ofertas);

        Assert.NotNull(resultado);
        Assert.Equal("Proveedor A", resultado.NombreProveedor);
    }

    private static OfertaBasica Oferta(
        Guid proveedorId,
        string nombreProveedor,
        decimal monto,
        DateTimeOffset fechaRegistro) =>
        new(Guid.NewGuid(), proveedorId, nombreProveedor, monto, fechaRegistro);

    private static DateTimeOffset Fecha(int hora) =>
        new(2026, 7, 24, hora, 0, 0, TimeSpan.Zero);
}
