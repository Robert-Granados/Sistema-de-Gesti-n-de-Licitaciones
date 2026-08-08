using Microsoft.Playwright;

namespace Licitaciones.BrowserTests;

/// <summary>
/// Flujo HU-46: creación, publicación y cierre de licitación.
/// </summary>
public sealed class LicitacionFlowTests : PageTest
{
    [Fact]
    public async Task CrearPublicarYCerrar_Licitacion_RecorreElCicloDeVidaCompleto()
    {
        var sufijo = TestSettings.SufijoUnico();
        var codigo = TestSettings.CodigoLicitacion(sufijo);
        var titulo = $"Licitación E2E {sufijo}";

        await Page.CrearLicitacionAsync(codigo, titulo, 1_000_000m);

        await Page.BuscarLicitacionAsync(codigo);
        var fila = Page.GetByRole(AriaRole.Row).Filter(new() { HasText = codigo });
        await Expect(fila).ToBeVisibleAsync();
        await Expect(fila).ToContainTextAsync("Borrador");

        await Page.AbrirDetalleLicitacionAsync(codigo);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = codigo, Exact = true }))
            .ToBeVisibleAsync();
        await Expect(Page.Locator("span.badge").Filter(new() { HasText = "Borrador" }))
            .ToBeVisibleAsync();

        await Page.PublicarAsync();
        await Expect(Page.Locator("span.badge").Filter(new() { HasText = "Publicada" }))
            .ToBeVisibleAsync();

        await Page.CerrarLicitacionAsync("Cierre de prueba E2E.");

        await Expect(Page.Locator("span.badge").Filter(new() { HasText = "Cerrada" }))
            .ToBeVisibleAsync();
        await Expect(Page.Locator("#ProveedorId")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Crear_LicitacionConCodigoDuplicado_MuestraMensajeDeError()
    {
        var codigo = TestSettings.CodigoLicitacion(TestSettings.SufijoUnico());
        var titulo = "Licitación duplicada E2E";

        await Page.CrearLicitacionAsync(codigo, titulo, 100_000m);

        await Page.IrAAsync("/Licitaciones/Crear");
        await Page.Locator("#Codigo").FillAsync(codigo);
        await Page.Locator("#Titulo").FillAsync(titulo);
        await Page.Locator("#FechaCierre").FillAsync(TestSettings.FechaCierreFutura());
        await Page.Locator("#PresupuestoEstimadoCrc").FillAsync(TestSettings.Monto(100_000m));
        await Page.GetByRole(AriaRole.Button, new() { Name = "Crear licitación" }).ClickAsync();

        await Page.EsperarErrorAsync("Ya existe una licitación registrada con ese código.");
    }
}
