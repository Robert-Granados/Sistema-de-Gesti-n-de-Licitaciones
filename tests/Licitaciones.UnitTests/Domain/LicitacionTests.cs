using Licitaciones.Domain.Entities;
using Licitaciones.Domain.Enums;

namespace Licitaciones.UnitTests.Domain;

public sealed class LicitacionTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 7, 23, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_ConDatosValidos_CreaLicitacionEnBorrador()
    {
        var licitacion = new Licitacion(" LIC-001 ", " Equipo de cómputo ", Ahora.AddDays(5), 1_000_000m);

        Assert.NotEqual(Guid.Empty, licitacion.Id);
        Assert.Equal("LIC-001", licitacion.Codigo);
        Assert.Equal("Equipo de cómputo", licitacion.Titulo);
        Assert.Equal(EstadoLicitacion.Borrador, licitacion.Estado);
        Assert.Equal(1_000_000m, licitacion.PresupuestoEstimadoCrc);
        Assert.False(typeof(Licitacion).GetProperty(nameof(Licitacion.Estado))!.SetMethod!.IsPublic);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ConCodigoVacio_LanzaExcepcion(string codigo)
    {
        Assert.Throws<ArgumentException>(() =>
            new Licitacion(codigo, "Título", Ahora.AddDays(1), 100m));
    }

    [Fact]
    public void Constructor_ConPresupuestoNoPositivo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 0m));
    }

    [Fact]
    public void Publicar_DesdeBorradorConFechaFutura_CambiaEstadoAPublicada()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);

        licitacion.Publicar(Ahora);

        Assert.Equal(EstadoLicitacion.Publicada, licitacion.Estado);
    }

    [Fact]
    public void Cerrar_DesdePublicada_CambiaEstadoACerrada()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);
        licitacion.Publicar(Ahora);

        licitacion.Cerrar();

        Assert.Equal(EstadoLicitacion.Cerrada, licitacion.Estado);
        Assert.Throws<InvalidOperationException>(() => licitacion.Publicar(Ahora));
    }
}
