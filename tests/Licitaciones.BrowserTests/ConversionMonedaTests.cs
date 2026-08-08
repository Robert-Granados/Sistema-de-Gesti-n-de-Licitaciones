using Microsoft.Playwright;

namespace Licitaciones.BrowserTests;

/// <summary>
/// Flujo HU-46: conversión de montos CRC/USD en el detalle de la licitación.
/// Usa el tipo de cambio local activo (se obtiene de la propia página para
/// no depender de un valor sembrado concreto en la base de datos).
/// </summary>
public sealed class ConversionMonedaTests : PageTest
{
    [Fact]
    public async Task Detalle_AlternaLaPresentacionEntreCRCyUSD()
    {
        var sufijo = TestSettings.SufijoUnico();
        var codigo = TestSettings.CodigoLicitacion(sufijo);
        const decimal presupuesto = 1_000_000m;

        await Page.CrearLicitacionAsync(codigo, $"Licitación de conversión {sufijo}", presupuesto);
        await Page.AbrirDetalleLicitacionAsync(codigo);

        var toggle = Page.Locator("[data-currency-toggle]");
        await Expect(toggle).ToBeVisibleAsync();

        var monto = Page.Locator("[data-currency-amount]").First;
        await Expect(monto).ToContainTextAsync("₡");
        await Expect(monto).Not.ToContainTextAsync("$");

        var tipoCambio = await toggle.GetAttributeAsync("data-exchange-rate");
        var crcPorUsd = double.Parse(
            tipoCambio!,
            System.Globalization.CultureInfo.InvariantCulture);
        var montoUsdEsperado = Math.Round((double)presupuesto / crcPorUsd, 2, MidpointRounding.AwayFromZero)
            .ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"));

        await Page.GetByRole(AriaRole.Button, new() { Name = "USD", Exact = true }).ClickAsync();

        await Expect(monto).ToHaveTextAsync(montoUsdEsperado);
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "CRC", Exact = true }))
            .ToHaveAttributeAsync("aria-pressed", "false");
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "USD", Exact = true }))
            .ToHaveAttributeAsync("aria-pressed", "true");

        await Page.GetByRole(AriaRole.Button, new() { Name = "CRC", Exact = true }).ClickAsync();

        await Expect(monto).ToContainTextAsync("₡");
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "USD", Exact = true }))
            .ToHaveAttributeAsync("aria-pressed", "false");
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "CRC", Exact = true }))
            .ToHaveAttributeAsync("aria-pressed", "true");
    }
}
