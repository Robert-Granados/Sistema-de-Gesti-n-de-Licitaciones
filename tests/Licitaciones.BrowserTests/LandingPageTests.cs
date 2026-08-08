using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Licitaciones.BrowserTests;

/// <summary>
/// Flujo HU-46: landing page y navegación principal.
/// </summary>
public sealed class LandingPageTests : PageTest
{
    [Fact]
    public async Task Landing_CargaConTituloYContenidoPrincipal()
    {
        await Page.GotoAsync(TestSettings.BaseUrl + "/");

        await Expect(Page).ToHaveTitleAsync("Inicio - Sistema de Licitaciones");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Licitaciones claras, decisiones respaldadas" }))
            .ToBeVisibleAsync();

        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Consultar licitaciones" }))
            .ToBeVisibleAsync();

        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Crear licitación" }))
            .ToBeVisibleAsync();
    }

    [Fact]
    public async Task Navegacion_RecorreTodosLosModulosDelMenu()
    {
        await Page.GotoAsync(TestSettings.BaseUrl + "/");

        var destinos = new Dictionary<string, string>
        {
            ["Licitaciones"] = "Licitaciones",
            ["Proveedores"] = "Proveedores",
            ["Ofertas"] = "Ofertas",
            ["Niveles de aprobación"] = "Niveles de aprobación",
            ["Tipo de cambio"] = "Tipo de cambio"
        };

        foreach (var (enlace, encabezado) in destinos)
        {
            await Page.GetByRole(AriaRole.Link, new() { Name = enlace, Exact = true }).First.ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = encabezado, Exact = true }))
                .ToBeVisibleAsync();
        }
    }

    [Fact]
    public async Task Navegacion_AccesosRapidosDelLandingConducenALosModulos()
    {
        await Page.GotoAsync(TestSettings.BaseUrl + "/");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Consultar licitaciones" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@"/Licitaciones/?$"));

        await Page.GotoAsync(TestSettings.BaseUrl + "/");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Crear licitación" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Crear licitación", Exact = true }))
            .ToBeVisibleAsync();
    }
}
