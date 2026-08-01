using Licitaciones.Application.Licitaciones.Detalle;
using Licitaciones.Application.Licitaciones.Ports;

namespace Licitaciones.UnitTests.Application.Licitaciones;

public sealed class CalculadorMejorOfertaTests
{
    [Fact]
    public void Calcular_SinOfertas_DevuelveNull()
    {
        var resultado = CalculadorMejorOferta.Calcular([]);

        Assert.Null(resultado);
    }

    [Fact]
    public void Calcular_ConOfertas_DevuelveLaDeMenorMonto()
    {
        var ofertas = new List<OfertaBasica>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Proveedor B", 1000m, Fecha(10)),
            new(Guid.NewGuid(), Guid.NewGuid(), "Proveedor A", 800m, Fecha(9))
        };

        var resultado = CalculadorMejorOferta.Calcular(ofertas);

        Assert.NotNull(resultado);
        Assert.Equal("Proveedor A", resultado.NombreProveedor);
        Assert.Equal(800m, resultado.MontoOfertadoCrc);
    }

    [Fact]
    public void Calcular_ConEmpateEnMonto_DesempataPorFechaDeRegistroMasAntigua()
    {
        var ofertas = new List<OfertaBasica>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Proveedor B", 800m, Fecha(10)),
            new(Guid.NewGuid(), Guid.NewGuid(), "Proveedor A", 800m, Fecha(9))
        };

        var resultado = CalculadorMejorOferta.Calcular(ofertas);

        Assert.NotNull(resultado);
        Assert.Equal("Proveedor A", resultado.NombreProveedor);
        Assert.Equal(Fecha(9), resultado.FechaRegistro);
    }

    private static DateTimeOffset Fecha(int hora) =>
        new(2026, 7, 24, hora, 0, 0, TimeSpan.Zero);
}
