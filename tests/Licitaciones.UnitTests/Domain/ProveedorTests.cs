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
}

