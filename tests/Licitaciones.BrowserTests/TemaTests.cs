using Microsoft.Playwright;

namespace Licitaciones.BrowserTests;

/// <summary>
/// Flujo HU-46: alternancia de tema claro/oscuro y su persistencia.
/// </summary>
public sealed class TemaTests : PageTest
{
    [Fact]
    public async Task AlternarTema_CambiaElTemaYPersisteAlRecargarYNavegar()
    {
        await Page.GotoAsync(TestSettings.BaseUrl + "/");

        await Expect(Page.Locator("html")).ToHaveAttributeAsync("data-theme", "light");

        await Page.Locator("#themeToggle").ClickAsync();

        await Expect(Page.Locator("html")).ToHaveAttributeAsync("data-theme", "dark");
        await Expect(Page.Locator("#themeToggle")).ToHaveAttributeAsync("aria-pressed", "true");
        await Expect(Page.Locator("#themeToggle [data-theme-label]")).ToHaveTextAsync("Modo claro");

        var guardado = await Page.EvaluateAsync<string>("localStorage.getItem('licitaciones-theme')");
        Assert.Equal("dark", guardado);

        await Page.ReloadAsync();
        await Expect(Page.Locator("html")).ToHaveAttributeAsync("data-theme", "dark");

        await Page.GotoAsync(TestSettings.BaseUrl + "/Proveedores");
        await Expect(Page.Locator("html")).ToHaveAttributeAsync("data-theme", "dark");

        await Page.Locator("#themeToggle").ClickAsync();
        await Expect(Page.Locator("html")).ToHaveAttributeAsync("data-theme", "light");
        await Expect(Page.Locator("#themeToggle [data-theme-label]")).ToHaveTextAsync("Modo oscuro");
    }
}
