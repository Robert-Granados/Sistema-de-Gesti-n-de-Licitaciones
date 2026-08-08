using Licitaciones.Domain.Entities;
using Licitaciones.Domain.Enums;
using Licitaciones.Domain.Exceptions;
using Licitaciones.UnitTests.Common;

namespace Licitaciones.UnitTests.Domain;

public sealed class TransicionEstadoLicitacionTests
{
    private static readonly FakeClock Clock =
        new(new(2026, 7, 24, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public void Constructor_IniciaEnBorrador()
    {
        var licitacion = CrearLicitacion();

        Assert.Equal(EstadoLicitacion.Borrador, licitacion.Estado);
    }

    [Fact]
    public void Publicar_DesdeBorrador_RegistraFechaDePublicacion()
    {
        var licitacion = CrearLicitacion();

        licitacion.Publicar(Clock.UtcNow);

        Assert.Equal(EstadoLicitacion.Publicada, licitacion.Estado);
        Assert.Equal(Clock.UtcNow, licitacion.PublicadaEn);
    }

    [Fact]
    public void Publicar_ConFechaDeCierreEnElPasado_Rechaza()
    {
        var licitacion = CrearLicitacion(fechaCierre: Clock.UtcNow.AddDays(-1));

        Assert.Throws<TransicionEstadoInvalidaException>(
            () => licitacion.Publicar(Clock.UtcNow));
    }

    [Fact]
    public void Publicar_ConFechaDeCierreIgualAhora_Rechaza()
    {
        var licitacion = CrearLicitacion(fechaCierre: Clock.UtcNow);

        Assert.Throws<TransicionEstadoInvalidaException>(
            () => licitacion.Publicar(Clock.UtcNow));
    }

    [Fact]
    public void Publicar_DesdePublicada_Rechaza()
    {
        var licitacion = CrearLicitacion();
        licitacion.Publicar(Clock.UtcNow);

        Assert.Throws<TransicionEstadoInvalidaException>(
            () => licitacion.Publicar(Clock.UtcNow));
    }

    [Fact]
    public void Publicar_DesdeCerrada_Rechaza()
    {
        var licitacion = CrearLicitacion();
        licitacion.Cerrar("Adjudicada a la oferta más baja", Clock.UtcNow);

        Assert.Throws<TransicionEstadoInvalidaException>(
            () => licitacion.Publicar(Clock.UtcNow));
    }

    [Fact]
    public void Cerrar_DesdePublicada_RegistraFechaYMotivo()
    {
        var licitacion = CrearLicitacion();
        licitacion.Publicar(Clock.UtcNow);

        licitacion.Cerrar("Adjudicada por mejor oferta", Clock.UtcNow);

        Assert.Equal(EstadoLicitacion.Cerrada, licitacion.Estado);
        Assert.Equal(Clock.UtcNow, licitacion.CerradaEn);
        Assert.Equal("Adjudicada por mejor oferta", licitacion.MotivoCierre);
    }

    [Fact]
    public void Cerrar_DesdeBorrador_RegistraFechaYMotivo()
    {
        var licitacion = CrearLicitacion();

        licitacion.Cerrar("Cancelada por falta de recursos", Clock.UtcNow);

        Assert.Equal(EstadoLicitacion.Cerrada, licitacion.Estado);
        Assert.Equal(Clock.UtcNow, licitacion.CerradaEn);
    }

    [Fact]
    public void Cerrar_DesdeCerrada_Rechaza()
    {
        var licitacion = CrearLicitacion();
        licitacion.Cerrar("Adjudicada", Clock.UtcNow);

        Assert.Throws<TransicionEstadoInvalidaException>(
            () => licitacion.Cerrar("Otro motivo", Clock.UtcNow));
    }

    [Fact]
    public void Cerrar_ConMotivoVacio_Rechaza()
    {
        var licitacion = CrearLicitacion();

        Assert.Throws<ArgumentException>(
            () => licitacion.Cerrar("   ", Clock.UtcNow));
    }

    [Fact]
    public void CambiarTitulo_DesdePublicada_ActualizaTitulo()
    {
        var licitacion = CrearLicitacion();
        licitacion.Publicar(Clock.UtcNow);

        licitacion.CambiarTitulo("Nuevo título");

        Assert.Equal("Nuevo título", licitacion.Titulo);
    }

    [Fact]
    public void CambiarTitulo_DesdeCerrada_Rechaza()
    {
        var licitacion = CrearLicitacion();
        licitacion.Cerrar("Adjudicada", Clock.UtcNow);

        Assert.Throws<TransicionEstadoInvalidaException>(
            () => licitacion.CambiarTitulo("Nuevo título"));
    }

    [Fact]
    public void Eliminar_RegistraEliminacionLogica()
    {
        var licitacion = CrearLicitacion();

        licitacion.Eliminar(Clock.UtcNow);

        Assert.True(licitacion.EstaEliminada);
        Assert.Equal(Clock.UtcNow, licitacion.EliminadoEn);
    }

    [Fact]
    public void Eliminar_DosVeces_Rechaza()
    {
        var licitacion = CrearLicitacion();
        licitacion.Eliminar(Clock.UtcNow);

        Assert.Throws<InvalidOperationException>(
            () => licitacion.Eliminar(Clock.UtcNow));
    }

    private static Licitacion CrearLicitacion(
        DateTimeOffset? fechaCierre = null,
        decimal presupuesto = 1_000_000m) =>
        new(
            "LIC-2026-001",
            "Servicios de consultoría",
            fechaCierre ?? Clock.UtcNow.AddDays(30),
            presupuesto);
}
