using Licitaciones.Domain.Entities;

namespace Licitaciones.UnitTests.Domain;

public sealed class NivelAprobacionTests
{
    [Fact]
    public void Constructor_ConRangoAbiertoValido_CreaNivel()
    {
        var nivel = new NivelAprobacion(10_000_000m, null, " Junta Directiva ");

        Assert.NotEqual(Guid.Empty, nivel.Id);
        Assert.Equal(10_000_000m, nivel.MontoMinimoCrc);
        Assert.Null(nivel.MontoMaximoCrc);
        Assert.Equal("Junta Directiva", nivel.Aprobador);
    }

    [Fact]
    public void Constructor_ConMaximoNoMayorAlMinimo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new NivelAprobacion(100m, 100m, "Gerencia"));
    }

    [Fact]
    public void Constructor_ConAprobadorVacio_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => new NivelAprobacion(0m, 100m, " "));
    }

    [Fact]
    public void Constructor_ConMontoMinimoNegativo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new NivelAprobacion(-1m, 100m, "Gerencia"));
    }

    [Fact]
    public void Actualizar_ConValoresValidos_ActualizaElNivel()
    {
        var nivel = new NivelAprobacion(10_000m, 100_000m, "Gerencia");

        nivel.Actualizar(50_000m, 200_000m, " Junta Directiva ");

        Assert.Equal(50_000m, nivel.MontoMinimoCrc);
        Assert.Equal(200_000m, nivel.MontoMaximoCrc);
        Assert.Equal("Junta Directiva", nivel.Aprobador);
    }

    [Fact]
    public void Actualizar_ConMontoMinimoNegativo_LanzaExcepcion()
    {
        var nivel = new NivelAprobacion(10_000m, 100_000m, "Gerencia");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            nivel.Actualizar(-1m, 100_000m, "Gerencia"));
    }

    [Fact]
    public void Actualizar_ConMaximoNoMayorAlMinimo_LanzaExcepcion()
    {
        var nivel = new NivelAprobacion(10_000m, 100_000m, "Gerencia");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            nivel.Actualizar(50_000m, 50_000m, "Gerencia"));
    }

    [Fact]
    public void Actualizar_ConAprobadorVacio_LanzaExcepcion()
    {
        var nivel = new NivelAprobacion(10_000m, 100_000m, "Gerencia");

        Assert.Throws<ArgumentException>(() =>
            nivel.Actualizar(10_000m, 100_000m, " "));
    }
}

