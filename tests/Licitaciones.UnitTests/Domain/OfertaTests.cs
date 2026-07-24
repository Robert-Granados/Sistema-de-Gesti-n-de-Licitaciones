using Licitaciones.Domain.Entities;

namespace Licitaciones.UnitTests.Domain;

public sealed class OfertaTests
{
    [Fact]
    public void Constructor_ConDatosValidos_CreaOferta()
    {
        var licitacionId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();
        var fechaRegistro = new DateTimeOffset(2026, 7, 23, 12, 0, 0, TimeSpan.Zero);

        var oferta = new Oferta(licitacionId, proveedorId, 500m, fechaRegistro);

        Assert.NotEqual(Guid.Empty, oferta.Id);
        Assert.Equal(licitacionId, oferta.LicitacionId);
        Assert.Equal(proveedorId, oferta.ProveedorId);
        Assert.Equal(500m, oferta.MontoOfertadoCrc);
        Assert.Equal(fechaRegistro, oferta.FechaRegistro);
    }

    [Fact]
    public void Constructor_ConMontoNoPositivo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Oferta(Guid.NewGuid(), Guid.NewGuid(), 0m, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Constructor_ConIdRelacionadoVacio_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            new Oferta(Guid.Empty, Guid.NewGuid(), 100m, DateTimeOffset.UtcNow));
    }
}

