using Microsoft.Playwright;

namespace Licitaciones.BrowserTests;

/// <summary>
/// Helpers de interacción con el navegador para los flujos de HU-46.
/// Operan sobre una página ya inicializada por Microsoft.Playwright.Xunit.
/// </summary>
internal static class UiExtensions
{
    public static async Task IrAAsync(this IPage page, string ruta)
    {
        await page.GotoAsync(TestSettings.BaseUrl + ruta);
    }

    public static async Task EsperarExitoAsync(this IPage page, string texto)
    {
        await EsperarNotificacionAsync(page, ".alert-success", texto);
    }

    public static async Task EsperarErrorAsync(this IPage page, string texto)
    {
        await EsperarNotificacionAsync(page, ".alert-danger", texto);
    }

    private static async Task EsperarNotificacionAsync(
        IPage page,
        string selector,
        string texto)
    {
        var alerta = page.Locator($"#notificaciones {selector}");
        await alerta.WaitForAsync();
        var contenido = await alerta.TextContentAsync() ?? string.Empty;
        Assert.Contains(texto, contenido);
    }

    public static async Task CrearProveedorAsync(this IPage page, string nombre)
    {
        await page.IrAAsync("/Proveedores/Crear");
        await page.Locator("#Nombre").FillAsync(nombre);
        await page.GetByRole(AriaRole.Button, new() { Name = "Registrar" }).ClickAsync();
        await page.EsperarExitoAsync("se registró correctamente");
    }

    public static async Task BuscarProveedorAsync(this IPage page, string nombre)
    {
        await page.IrAAsync("/Proveedores");
        await page.Locator("#search").FillAsync(nombre);
        await page.GetByRole(AriaRole.Button, new() { Name = "Aplicar" }).ClickAsync();
    }

    public static async Task AbrirDetalleProveedorAsync(this IPage page, string nombre)
    {
        await page.BuscarProveedorAsync(nombre);
        await page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = nombre })
            .GetByRole(AriaRole.Link, new() { Name = "Ver detalle" })
            .ClickAsync();
    }

    public static async Task CrearLicitacionAsync(
        this IPage page,
        string codigo,
        string titulo,
        decimal presupuestoCrc,
        string? fechaCierre = null)
    {
        await page.IrAAsync("/Licitaciones/Crear");
        await page.Locator("#Codigo").FillAsync(codigo);
        await page.Locator("#Titulo").FillAsync(titulo);
        await page.Locator("#FechaCierre").FillAsync(fechaCierre ?? TestSettings.FechaCierreFutura());
        await page.Locator("#PresupuestoEstimadoCrc").FillAsync(TestSettings.Monto(presupuestoCrc));
        await page.GetByRole(AriaRole.Button, new() { Name = "Crear licitación" }).ClickAsync();
        await page.EsperarExitoAsync("se registró correctamente");
    }

    public static async Task BuscarLicitacionAsync(this IPage page, string codigo)
    {
        await page.IrAAsync("/Licitaciones");
        await page.Locator("#search").FillAsync(codigo);
        await page.GetByRole(AriaRole.Button, new() { Name = "Aplicar" }).ClickAsync();
    }

    public static async Task AbrirDetalleLicitacionAsync(this IPage page, string codigo)
    {
        await page.BuscarLicitacionAsync(codigo);
        await page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = codigo })
            .GetByRole(AriaRole.Link, new() { Name = "Ver detalle" })
            .ClickAsync();
    }

    public static async Task PublicarAsync(this IPage page)
    {
        page.Dialog += async (_, dialogo) => await dialogo.AcceptAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Publicar" }).ClickAsync();
        await page.EsperarExitoAsync("se publicó correctamente");
    }

    public static async Task AbrirModalCierreAsync(this IPage page)
    {
        await page.Locator("[data-bs-target='#cerrarModal']").ClickAsync();
        await page.Locator("#cerrarModal").WaitForAsync();
    }

    public static async Task CerrarLicitacionAsync(
        this IPage page,
        string motivo)
    {
        await page.AbrirModalCierreAsync();
        await page.Locator("#motivo").FillAsync(motivo);
        await page.GetByRole(AriaRole.Button, new() { Name = "Cerrar licitación" }).ClickAsync();
        await page.EsperarExitoAsync("se cerró correctamente");
    }

    public static async Task LlenarFormularioOfertaAsync(
        this IPage page,
        string nombreProveedor,
        decimal montoCrc)
    {
        await page.Locator("#ProveedorId")
            .SelectOptionAsync(new SelectOptionValue { Label = nombreProveedor });
        await page.Locator("#MontoOfertadoCrc").FillAsync(TestSettings.Monto(montoCrc));
        await page.GetByRole(AriaRole.Button, new() { Name = "Registrar oferta" }).ClickAsync();
    }

    public static async Task FiltrarOfertasPorLicitacionAsync(
        this IPage page,
        string codigoLicitacion)
    {
        await page.Locator("#licitacionId")
            .SelectOptionAsync(new SelectOptionValue { Label = codigoLicitacion });
        await page.GetByRole(AriaRole.Button, new() { Name = "Aplicar" }).ClickAsync();
    }

    public static async Task ConfirmarEliminacionAsync(this IPage page)
    {
        await page.Locator("#confirmDeleteModal").WaitForAsync();
        await page.Locator("#confirmDeleteButton").ClickAsync();
    }
}
