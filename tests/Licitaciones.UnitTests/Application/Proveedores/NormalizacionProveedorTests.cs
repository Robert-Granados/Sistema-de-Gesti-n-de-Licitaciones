using Licitaciones.Application.Proveedores.Crear;

namespace Licitaciones.UnitTests.Application.Proveedores;

public sealed class NormalizacionProveedorTests
{
    [Fact]
    public void Limpiar_ConEspaciosInicialesYFinales_LimpiaElNombre()
    {
        Assert.Equal("Tecnología y Más S.A.", NombreProveedorNormalizer.Limpiar("  Tecnología y Más S.A.  "));
    }

    [Fact]
    public void Limpiar_ConEspaciosRepetidos_ColapsaAUnoSolo()
    {
        Assert.Equal("Tecnología Global", NombreProveedorNormalizer.Limpiar("Tecnología   Global"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Limpiar_NuloOVacio_DevuelveCadenaVacia(string? nombre)
    {
        Assert.Equal(string.Empty, NombreProveedorNormalizer.Limpiar(nombre));
    }

    [Fact]
    public void EsValido_ConNombreValido_DevuelveTrue()
    {
        Assert.True(NombreProveedorNormalizer.EsValido("Tecnología y Más S.A. (Sucursal 2)"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EsValido_ConNombreVacio_DevuelveFalse(string nombre)
    {
        Assert.False(NombreProveedorNormalizer.EsValido(nombre));
    }

    [Fact]
    public void EsValido_ConCaracteresNoPermitidos_DevuelveFalse()
    {
        Assert.False(NombreProveedorNormalizer.EsValido("Empresa@123"));
    }

    [Fact]
    public void EsValido_ConLongitudMayorADoscientos_DevuelveFalse()
    {
        var nombreLargo = new string('A', 201);

        Assert.False(NombreProveedorNormalizer.EsValido(nombreLargo));
    }

    [Fact]
    public void Normalizar_EliminaTildesYConvierteAMayusculas()
    {
        var resultado = NombreProveedorNormalizer.Normalizar("Sistemas Águila S.A.");

        Assert.Equal("SISTEMAS AGUILA S.A.", resultado);
    }

    [Fact]
    public void Normalizar_ColapsaEspaciosYQuitaEspaciosExtremos()
    {
        var resultado = NombreProveedorNormalizer.Normalizar("  Tecnologia   Global  ");

        Assert.Equal("TECNOLOGIA GLOBAL", resultado);
    }
}
