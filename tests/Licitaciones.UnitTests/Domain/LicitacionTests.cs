using Licitaciones.Domain.Entities;
using Licitaciones.Domain.Enums;
using Licitaciones.Domain.Exceptions;

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
    public void Constructor_ConFechaCierrePorDefecto_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            new Licitacion("LIC-001", "Título", default, 100m));
    }

    [Fact]
    public void Publicar_DesdeBorradorConFechaFutura_CambiaEstadoAPublicada()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);

        licitacion.Publicar(Ahora);

        Assert.Equal(EstadoLicitacion.Publicada, licitacion.Estado);
        Assert.Equal(Ahora, licitacion.PublicadaEn);
    }

    [Fact]
    public void Publicar_DesdePublicada_LanzaExcepcion()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);
        licitacion.Publicar(Ahora);

        var ex = Assert.Throws<TransicionEstadoInvalidaException>(
            () => licitacion.Publicar(Ahora));

        Assert.Contains("Publicada", ex.Message);
    }

    [Fact]
    public void Publicar_DesdeCerrada_LanzaExcepcion()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);
        licitacion.Cerrar("Cancelación", Ahora);

        var ex = Assert.Throws<TransicionEstadoInvalidaException>(
            () => licitacion.Publicar(Ahora));

        Assert.Contains("Cerrada", ex.Message);
    }

    [Fact]
    public void Publicar_ConFechaCierrePasada_LanzaExcepcion()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(-1), 100m);

        Assert.Throws<TransicionEstadoInvalidaException>(
            () => licitacion.Publicar(Ahora));
    }

    [Fact]
    public void Cerrar_DesdePublicadaConMotivo_CambiaEstadoACerrada()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);
        licitacion.Publicar(Ahora);

        licitacion.Cerrar("Fecha de cierre alcanzada", Ahora);

        Assert.Equal(EstadoLicitacion.Cerrada, licitacion.Estado);
        Assert.Equal("Fecha de cierre alcanzada", licitacion.MotivoCierre);
        Assert.Equal(Ahora, licitacion.CerradaEn);
    }

    [Fact]
    public void Cerrar_DesdeBorradorConMotivo_CambiaEstadoACerrada()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);

        licitacion.Cerrar("Cancelada por el usuario", Ahora);

        Assert.Equal(EstadoLicitacion.Cerrada, licitacion.Estado);
        Assert.Equal("Cancelada por el usuario", licitacion.MotivoCierre);
        Assert.Equal(Ahora, licitacion.CerradaEn);
    }

    [Fact]
    public void Cerrar_DesdeCerrada_LanzaExcepcion()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);
        licitacion.Cerrar("Primera razón", Ahora);

        var ex = Assert.Throws<TransicionEstadoInvalidaException>(
            () => licitacion.Cerrar("Segunda razón", Ahora));

        Assert.Contains("Cerrada", ex.Message);
    }

    [Fact]
    public void Cerrar_ConMotivoVacio_LanzaExcepcion()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);

        Assert.Throws<ArgumentException>(
            () => licitacion.Cerrar("", Ahora));
    }

    [Fact]
    public void Cerrar_ConMotivoNulo_LanzaExcepcion()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);

        Assert.Throws<ArgumentException>(
            () => licitacion.Cerrar(null!, Ahora));
    }

    [Fact]
    public void Cerrar_ConMotivoEspacios_Trimmed()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);

        licitacion.Cerrar("  Motivo con espacios  ", Ahora);

        Assert.Equal("Motivo con espacios", licitacion.MotivoCierre);
    }

    [Fact]
    public void CambiarTitulo_DesdePublicada_Permitido()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);
        licitacion.Publicar(Ahora);

        licitacion.CambiarTitulo("Nuevo título");

        Assert.Equal("Nuevo título", licitacion.Titulo);
    }

    [Fact]
    public void CambiarTitulo_DesdeCerrada_LanzaExcepcion()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);
        licitacion.Cerrar("Cancelación", Ahora);

        Assert.Throws<TransicionEstadoInvalidaException>(
            () => licitacion.CambiarTitulo("Nuevo título"));
    }

    [Fact]
    public void Publicar_DesdeBorradorCreaTransicionCorrecta()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(5), 500_000m);

        Assert.Equal(EstadoLicitacion.Borrador, licitacion.Estado);
        Assert.Null(licitacion.PublicadaEn);

        licitacion.Publicar(Ahora);

        Assert.Equal(EstadoLicitacion.Publicada, licitacion.Estado);
        Assert.Equal(Ahora, licitacion.PublicadaEn);
        Assert.Null(licitacion.CerradaEn);
        Assert.Null(licitacion.MotivoCierre);
    }

    [Theory]
    [InlineData(EstadoLicitacion.Publicada)]
    [InlineData(EstadoLicitacion.Borrador)]
    public void Cerrar_DesdeEstadoValido_CambiaACerrada(EstadoLicitacion estadoInicial)
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);

        if (estadoInicial == EstadoLicitacion.Publicada)
        {
            licitacion.Publicar(Ahora);
        }

        licitacion.Cerrar("Motivo de cierre", Ahora);

        Assert.Equal(EstadoLicitacion.Cerrada, licitacion.Estado);
    }

    [Fact]
    public void Eliminar_ConFechaValida_MarcaComoEliminada()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);

        licitacion.Eliminar(Ahora);

        Assert.True(licitacion.EstaEliminada);
        Assert.Equal(Ahora, licitacion.EliminadoEn);
    }

    [Fact]
    public void Eliminar_DobleEliminacion_LanzaExcepcion()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);
        licitacion.Eliminar(Ahora);

        Assert.Throws<InvalidOperationException>(
            () => licitacion.Eliminar(Ahora.AddDays(1)));
    }

    [Fact]
    public void Eliminar_DesdePublicada_Permitido()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);
        licitacion.Publicar(Ahora);

        licitacion.Eliminar(Ahora);

        Assert.True(licitacion.EstaEliminada);
    }

    [Fact]
    public void Eliminar_DesdeCerrada_Permitido()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);
        licitacion.Cerrar("Motivo", Ahora);

        licitacion.Eliminar(Ahora);

        Assert.True(licitacion.EstaEliminada);
    }

    [Fact]
    public void ActualizarFechaCierre_ConFechaValida_ActualizaElCierre()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);

        licitacion.ActualizarFechaCierre(Ahora.AddDays(10));

        Assert.Equal(Ahora.AddDays(10), licitacion.FechaCierre);
    }

    [Fact]
    public void ActualizarFechaCierre_ConFechaPorDefecto_LanzaExcepcion()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);

        Assert.Throws<ArgumentException>(() =>
            licitacion.ActualizarFechaCierre(default));
    }

    [Fact]
    public void ActualizarPresupuesto_ConValorValido_ActualizaElPresupuesto()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);

        licitacion.ActualizarPresupuesto(250_000m);

        Assert.Equal(250_000m, licitacion.PresupuestoEstimadoCrc);
    }

    [Fact]
    public void ActualizarPresupuesto_ConValorNoPositivo_LanzaExcepcion()
    {
        var licitacion = new Licitacion("LIC-001", "Título", Ahora.AddDays(1), 100m);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            licitacion.ActualizarPresupuesto(0m));
    }
}
