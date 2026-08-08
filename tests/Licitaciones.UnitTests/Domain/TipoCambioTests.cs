using Licitaciones.Domain.Entities;

namespace Licitaciones.UnitTests.Domain;

public sealed class TipoCambioTests
{
    [Fact]
    public void Constructor_ConDatosValidos_CreaTipoCambioInactivo()
    {
        var fecha = new DateTimeOffset(2026, 7, 23, 0, 0, 0, TimeSpan.Zero);

        var tipoCambio = new TipoCambio(505.25m, fecha);

        Assert.NotEqual(Guid.Empty, tipoCambio.Id);
        Assert.Equal(505.25m, tipoCambio.CrcPorUsd);
        Assert.Equal(fecha, tipoCambio.FechaVigencia);
        Assert.False(tipoCambio.Activo);
    }

    [Fact]
    public void Constructor_ConValorNoPositivo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new TipoCambio(0m, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Activar_CambiaEstadoSinExponerSetterPublico()
    {
        var tipoCambio = new TipoCambio(505.25m, DateTimeOffset.UtcNow);

        tipoCambio.Activar();

        Assert.True(tipoCambio.Activo);
        Assert.False(typeof(TipoCambio).GetProperty(nameof(TipoCambio.Activo))!.SetMethod!.IsPublic);
    }

    [Fact]
    public void Actualizar_ConValorNoPositivo_EsRechazado()
    {
        var tipoCambio = new TipoCambio(505.25m, DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            tipoCambio.Actualizar(-1m, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Constructor_ConFechaPorDefecto_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            new TipoCambio(505.25m, default));
    }

    [Fact]
    public void Actualizar_ConValoresValidos_ActualizaElTipoCambio()
    {
        var tipoCambio = new TipoCambio(505.25m, DateTimeOffset.UtcNow);
        var nuevaFecha = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);

        tipoCambio.Actualizar(512.75m, nuevaFecha);

        Assert.Equal(512.75m, tipoCambio.CrcPorUsd);
        Assert.Equal(nuevaFecha, tipoCambio.FechaVigencia);
    }

    [Fact]
    public void Actualizar_ConFechaPorDefecto_LanzaExcepcion()
    {
        var tipoCambio = new TipoCambio(505.25m, DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(() =>
            tipoCambio.Actualizar(512.75m, default));
    }
}
