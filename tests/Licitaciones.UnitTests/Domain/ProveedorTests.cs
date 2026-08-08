using Licitaciones.Domain.Entities;

namespace Licitaciones.UnitTests.Domain;

public sealed class ProveedorTests
{
    [Fact]
    public void Constructor_ConNombreValido_CreaProveedorActivo()
    {
        var proveedor = new Proveedor(" Proveedor Uno ");

        Assert.NotEqual(Guid.Empty, proveedor.Id);
        Assert.Equal("Proveedor Uno", proveedor.Nombre);
        Assert.False(proveedor.EstaEliminado);
    }

    [Fact]
    public void Constructor_ConNombreVacio_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => new Proveedor(" "));
    }

    [Fact]
    public void Constructor_ConCaracteresNoPermitidos_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => new Proveedor("Proveedor <malicioso>"));
    }

    [Fact]
    public void Constructor_ConNormalizadoVacio_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            new Proveedor("Proveedor Uno", " "));
    }

    [Fact]
    public void CambiarNombre_ConValoresValidos_ActualizaElNombre()
    {
        var proveedor = new Proveedor("Proveedor Uno");

        proveedor.CambiarNombre(" Proveedor Dos ", "PROVEEDOR DOS");

        Assert.Equal("Proveedor Dos", proveedor.Nombre);
        Assert.Equal("PROVEEDOR DOS", proveedor.NombreNormalizado);
    }

    [Fact]
    public void CambiarNombre_ConNombreVacio_LanzaExcepcion()
    {
        var proveedor = new Proveedor("Proveedor Uno");

        Assert.Throws<ArgumentException>(() =>
            proveedor.CambiarNombre(" ", "PROVEEDOR UNO"));
    }

    [Fact]
    public void CambiarNombre_ConCaracteresNoPermitidos_LanzaExcepcion()
    {
        var proveedor = new Proveedor("Proveedor Uno");

        Assert.Throws<ArgumentException>(() =>
            proveedor.CambiarNombre("Proveedor <malicioso>", "PROVEEDOR UNO"));
    }

    [Fact]
    public void CambiarNombre_ConNormalizadoVacio_LanzaExcepcion()
    {
        var proveedor = new Proveedor("Proveedor Uno");

        Assert.Throws<ArgumentException>(() =>
            proveedor.CambiarNombre("Proveedor Dos", " "));
    }

    [Fact]
    public void Eliminar_DobleEliminacion_LanzaExcepcion()
    {
        var proveedor = new Proveedor("Proveedor Uno");
        proveedor.Eliminar(DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            proveedor.Eliminar(DateTimeOffset.UtcNow.AddMinutes(1)));
    }
}

