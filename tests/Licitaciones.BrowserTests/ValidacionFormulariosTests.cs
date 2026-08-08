using Microsoft.Playwright;

namespace Licitaciones.BrowserTests;

/// <summary>
/// Flujo HU-46: mensajes de validación de los formularios.
/// </summary>
public sealed class ValidacionFormulariosTests : PageTest
{
    [Fact]
    public async Task Proveedor_ConFormularioVacio_MuestraErrorDeObligatoriedad()
    {
        await Page.IrAAsync("/Proveedores/Crear");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Registrar" }).ClickAsync();

        await Expect(Page.Locator("#Nombre-error"))
            .ToHaveTextAsync("El nombre del proveedor es obligatorio.");
    }

    [Fact]
    public async Task Proveedor_ConNombreNoPermitido_MuestraErrorDeFormato()
    {
        await Page.IrAAsync("/Proveedores/Crear");
        await Page.Locator("#Nombre").FillAsync("Proveedor @ inválido");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Registrar" }).ClickAsync();

        await Expect(Page.Locator("#Nombre-error")).ToHaveTextAsync(
            "El nombre solo puede contener letras, números, espacios, punto, coma o paréntesis.");
    }

    [Fact]
    public async Task Licitacion_ConFormularioVacio_MuestraErroresDeObligatoriedad()
    {
        await Page.IrAAsync("/Licitaciones/Crear");
        await Page.Locator("#FechaCierre").FillAsync(string.Empty);
        await Page.Locator("#PresupuestoEstimadoCrc").FillAsync(string.Empty);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Crear licitación" }).ClickAsync();

        await Expect(Page.Locator("#Codigo-error"))
            .ToHaveTextAsync("El código de la licitación es obligatorio.");
        await Expect(Page.Locator("#Titulo-error"))
            .ToHaveTextAsync("El título de la licitación es obligatorio.");
        await Expect(Page.Locator("#FechaCierre-error"))
            .ToHaveTextAsync("La fecha de cierre es obligatoria.");
        await Expect(Page.Locator("#PresupuestoEstimadoCrc-error"))
            .ToHaveTextAsync("El presupuesto estimado es obligatorio.");
    }

    [Fact]
    public async Task Licitacion_ConPresupuestoCero_MuestraErrorDeRango()
    {
        await Page.IrAAsync("/Licitaciones/Crear");
        await Page.Locator("#Codigo").FillAsync("LIC-VAL-001");
        await Page.Locator("#Titulo").FillAsync("Licitación con presupuesto inválido");
        await Page.Locator("#FechaCierre").FillAsync(TestSettings.FechaCierreFutura());
        await Page.Locator("#PresupuestoEstimadoCrc").FillAsync("0");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Crear licitación" }).ClickAsync();

        await Expect(Page.Locator("#PresupuestoEstimadoCrc-error"))
            .ToHaveTextAsync("El presupuesto estimado debe ser mayor que cero.");
    }
}
