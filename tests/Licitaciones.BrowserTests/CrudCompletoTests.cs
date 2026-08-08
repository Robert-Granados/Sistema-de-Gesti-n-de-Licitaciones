using Microsoft.Playwright;

namespace Licitaciones.BrowserTests;

/// <summary>
/// Flujo HU-46: CRUD completo de proveedores, licitaciones y ofertas
/// desde el navegador (crear, leer, actualizar y eliminar).
/// </summary>
public sealed class CrudCompletoTests : PageTest
{
    [Fact]
    public async Task Crud_Proveedor_CompletoDesdeNavegador()
    {
        var sufijo = TestSettings.SufijoUnico();
        var nombre = TestSettings.NombreProveedor(sufijo);
        var nombreEditado = $"Proveedor E2E Borrado {sufijo}";

        await Page.CrearProveedorAsync(nombre);

        await Page.BuscarProveedorAsync(nombre);
        await Expect(Page.GetByRole(AriaRole.Row).Filter(new() { HasText = nombre }))
            .ToBeVisibleAsync();

        await Page.AbrirDetalleProveedorAsync(nombre);
        await Page.GetByRole(AriaRole.Link, new() { Name = "Editar" }).ClickAsync();
        await Page.Locator("#Nombre").FillAsync(nombreEditado);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Guardar cambios" }).ClickAsync();
        await Page.EsperarExitoAsync("se actualizó correctamente");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Eliminar", Exact = true }).ClickAsync();
        await Page.ConfirmarEliminacionAsync();
        await Page.EsperarExitoAsync("se eliminó lógicamente");

        await Page.BuscarProveedorAsync(nombreEditado);
        await Expect(Page.GetByRole(AriaRole.Status)
            .Filter(new() { HasText = "No se encontraron proveedores" }))
            .ToBeVisibleAsync();
    }

    [Fact]
    public async Task Crud_Licitacion_CompletoDesdeNavegador()
    {
        var sufijo = TestSettings.SufijoUnico();
        var codigo = TestSettings.CodigoLicitacion(sufijo);
        var titulo = $"Licitación CRUD {sufijo}";
        var tituloEditado = $"Licitación CRUD Editada {sufijo}";

        await Page.CrearLicitacionAsync(codigo, titulo, 750_000m);

        await Page.BuscarLicitacionAsync(codigo);
        await Expect(Page.GetByRole(AriaRole.Row).Filter(new() { HasText = codigo }))
            .ToBeVisibleAsync();

        await Page.AbrirDetalleLicitacionAsync(codigo);
        await Page.GetByRole(AriaRole.Link, new() { Name = "Editar" }).ClickAsync();
        await Page.Locator("#Titulo").FillAsync(tituloEditado);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Guardar cambios" }).ClickAsync();
        await Page.EsperarExitoAsync("se actualizó correctamente");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = codigo, Exact = true }))
            .ToBeVisibleAsync();
        await Expect(Page.Locator("body")).ToContainTextAsync(tituloEditado);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Eliminar", Exact = true }).ClickAsync();
        await Page.ConfirmarEliminacionAsync();
        await Page.EsperarExitoAsync("se eliminó correctamente");

        await Page.BuscarLicitacionAsync(codigo);
        await Expect(Page.GetByRole(AriaRole.Status)
            .Filter(new() { HasText = "No se encontraron licitaciones" }))
            .ToBeVisibleAsync();
    }

    [Fact]
    public async Task Crud_Oferta_CompletoDesdeNavegador()
    {
        var sufijo = TestSettings.SufijoUnico();
        var codigo = TestSettings.CodigoLicitacion(sufijo);
        var proveedor = $"Proveedor Oferta CRUD {sufijo}";

        await Page.CrearProveedorAsync(proveedor);
        await Page.CrearLicitacionAsync(codigo, $"Licitación oferta CRUD {sufijo}", 1_000_000m);
        await Page.AbrirDetalleLicitacionAsync(codigo);
        await Page.PublicarAsync();

        await Page.LlenarFormularioOfertaAsync(proveedor, 500_000m);
        await Page.EsperarExitoAsync("La oferta se registró correctamente.");

        await Page.IrAAsync("/Ofertas");
        await Page.FiltrarOfertasPorLicitacionAsync(codigo);
        var fila = Page.GetByRole(AriaRole.Row).Filter(new() { HasText = codigo });
        await Expect(fila).ToBeVisibleAsync();
        await Expect(fila).ToContainTextAsync(proveedor);

        await fila.GetByRole(AriaRole.Link, new() { Name = "Editar" }).ClickAsync();
        await Page.Locator("#MontoOfertadoCrc").FillAsync(TestSettings.Monto(450_000m));
        await Page.GetByRole(AriaRole.Button, new() { Name = "Guardar cambios" }).ClickAsync();
        await Page.EsperarExitoAsync("se actualizó correctamente");

        await Page.FiltrarOfertasPorLicitacionAsync(codigo);
        var filaEditada = Page.GetByRole(AriaRole.Row).Filter(new() { HasText = codigo });
        await Expect(filaEditada).ToBeVisibleAsync();
        var montoEsperado = 450_000m.ToString(
            "N2",
            System.Globalization.CultureInfo.GetCultureInfo("es-CR"))
            .Replace('\u00A0', ' ');
        var textoFila = (await filaEditada.TextContentAsync())?
            .Replace('\u00A0', ' ')
            .Replace('\u202F', ' ') ?? string.Empty;
        Assert.Contains(montoEsperado, textoFila);

        await filaEditada.GetByRole(AriaRole.Button, new() { Name = "Eliminar", Exact = true }).ClickAsync();
        await Page.ConfirmarEliminacionAsync();
        await Page.EsperarExitoAsync("La oferta se eliminó correctamente.");

        await Page.FiltrarOfertasPorLicitacionAsync(codigo);
        await Expect(Page.GetByRole(AriaRole.Row).Filter(new() { HasText = codigo }))
            .ToHaveCountAsync(0);
    }
}
