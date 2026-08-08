using Licitaciones.Application.Licitaciones.Detalle;
using Licitaciones.Domain.Entities;

namespace Licitaciones.UnitTests.Application.Licitaciones;

public sealed class ConversionMonedaTests
{
    private static readonly DateTimeOffset FechaVigencia =
        new(2026, 8, 2, 10, 0, 0, TimeSpan.FromHours(-6));

    [Fact]
    public void ConvertirAUsd_SinTipoDeCambioActivo_DevuelveNull()
    {
        var resultado = ConversionMonedaService.ConvertirAUsd(null, 1_000_000m);

        Assert.Null(resultado);
    }

    [Fact]
    public void ConvertirAUsd_ConTipoDeCambioSinValor_DevuelveNull()
    {
        var tipoCambio = new TipoCambio(500m, FechaVigencia);
        typeof(TipoCambio)
            .GetProperty(nameof(TipoCambio.CrcPorUsd))!
            .SetValue(tipoCambio, 0m);

        var resultado = ConversionMonedaService.ConvertirAUsd(tipoCambio, 1_000_000m);

        Assert.Null(resultado);
    }

    [Fact]
    public void ConvertirAUsd_ConMontoNoPositivo_DevuelveNull()
    {
        var resultado = ConversionMonedaService.ConvertirAUsd(
            new TipoCambio(500m, FechaVigencia),
            0m);

        Assert.Null(resultado);
    }

    [Fact]
    public void ConvertirAUsd_ConTipoDeCambioValido_ConvierteMonto()
    {
        var resultado = ConversionMonedaService.ConvertirAUsd(
            new TipoCambio(500m, FechaVigencia),
            1_000_000m);

        Assert.NotNull(resultado);
        Assert.Equal(2000m, resultado.Value.MontoUsd);
    }

    [Fact]
    public void ConvertirAUsd_RedondeaElMontoADosDecimales()
    {
        var resultado = ConversionMonedaService.ConvertirAUsd(
            new TipoCambio(3m, FechaVigencia),
            1m);

        Assert.NotNull(resultado);
        Assert.Equal(0.33m, resultado.Value.MontoUsd);
    }

    [Fact]
    public void ConvertirAUsd_DevuelveLaFechaDeVigenciaDelTipoDeCambio()
    {
        var resultado = ConversionMonedaService.ConvertirAUsd(
            new TipoCambio(500m, FechaVigencia),
            1_000_000m);

        Assert.NotNull(resultado);
        Assert.Equal(FechaVigencia, resultado.Value.FechaVigencia);
    }
}
