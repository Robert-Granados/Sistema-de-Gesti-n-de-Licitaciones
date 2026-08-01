using Licitaciones.Application.Licitaciones.Detalle;

namespace Licitaciones.UnitTests.Application.Licitaciones;

public sealed class ClasificadorAhorroTests
{
    [Theory]
    [InlineData(1_000_000, 900_000)] // 10% exacto
    [InlineData(1_000_000, 850_000)] // 15%
    public void Clasificar_AhorroMayorOIgualAlDiezPorCiento_DevuelveOfertaConveniente(
        decimal presupuesto,
        decimal monto)
    {
        var clasificacion = ClasificadorAhorro.Clasificar(presupuesto, monto);

        Assert.Equal(ClasificacionAhorro.OfertaConveniente, clasificacion);
    }

    [Fact]
    public void Clasificar_AhorroEntreCeroYDiezPorCiento_DevuelveOfertaAceptable()
    {
        var clasificacion = ClasificadorAhorro.Clasificar(1_000_000m, 950_000m);

        Assert.Equal(ClasificacionAhorro.OfertaAceptable, clasificacion);
    }

    [Fact]
    public void Clasificar_OfertaIgualAlPresupuesto_DevuelveOfertaValidaSinAhorro()
    {
        var clasificacion = ClasificadorAhorro.Clasificar(1_000_000m, 1_000_000m);

        Assert.Equal(ClasificacionAhorro.OfertaValidaSinAhorro, clasificacion);
    }

    [Fact]
    public void Clasificar_OfertaMayorAlPresupuesto_DevuelveOfertaValidaSinAhorro()
    {
        var clasificacion = ClasificadorAhorro.Clasificar(1_000_000m, 1_100_000m);

        Assert.Equal(ClasificacionAhorro.OfertaValidaSinAhorro, clasificacion);
    }
}
