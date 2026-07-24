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
}

