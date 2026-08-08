using Microsoft.Playwright;

namespace Licitaciones.BrowserTests;

/// <summary>
/// Flujo HU-46: creación y edición de proveedor.
/// </summary>
public sealed class ProveedorFlowTests : PageTest
{
    [Fact]
    public async Task CrearYEditar_Proveedor_ReflejaLosCambiosEnListadoYDetalle()
    {
        var sufijo = TestSettings.SufijoUnico();
        var nombreOriginal = TestSettings.NombreProveedor(sufijo);
        var nombreEditado = $"Proveedor E2E Editado {sufijo}";

        await Page.CrearProveedorAsync(nombreOriginal);

        await Page.BuscarProveedorAsync(nombreOriginal);
        await Expect(Page.GetByRole(AriaRole.Row).Filter(new() { HasText = nombreOriginal }))
            .ToBeVisibleAsync();

        await Page.AbrirDetalleProveedorAsync(nombreOriginal);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = nombreOriginal, Exact = true }))
            .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Editar" }).ClickAsync();
        await Page.Locator("#Nombre").FillAsync(nombreEditado);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Guardar cambios" }).ClickAsync();
        await Page.EsperarExitoAsync("se actualizó correctamente");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = nombreEditado, Exact = true }))
            .ToBeVisibleAsync();

        await Page.BuscarProveedorAsync(nombreEditado);
        await Expect(Page.GetByRole(AriaRole.Row).Filter(new() { HasText = nombreEditado }))
            .ToBeVisibleAsync();
    }

    [Fact]
    public async Task Crear_ProveedorDuplicado_MuestraMensajeDeError()
    {
        var nombre = TestSettings.NombreProveedor(TestSettings.SufijoUnico());

        await Page.CrearProveedorAsync(nombre);

        await Page.IrAAsync("/Proveedores/Crear");
        await Page.Locator("#Nombre").FillAsync(nombre);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Registrar" }).ClickAsync();

        await Page.EsperarErrorAsync("Ya existe un proveedor registrado con ese nombre.");
    }
}
