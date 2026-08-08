using Microsoft.Playwright;

namespace Licitaciones.BrowserTests;

/// <summary>
/// Flujo HU-46: registro y rechazo de ofertas.
/// </summary>
public sealed class OfertaFlowTests : PageTest
{
    [Fact]
    public async Task RegistrarOfertas_DeterminaMejorPropuesta_YRechazaDuplicada()
    {
        var sufijo = TestSettings.SufijoUnico();
        var codigo = TestSettings.CodigoLicitacion(sufijo);
        var proveedorA = $"Proveedor Oferta A {sufijo}";
        var proveedorB = $"Proveedor Oferta B {sufijo}";

        await Page.CrearProveedorAsync(proveedorA);
        await Page.CrearProveedorAsync(proveedorB);
        await Page.CrearLicitacionAsync(codigo, $"Licitación de ofertas {sufijo}", 1_000_000m);
        await Page.AbrirDetalleLicitacionAsync(codigo);
        await Page.PublicarAsync();

        await Page.LlenarFormularioOfertaAsync(proveedorA, 500_000m);
        await Page.EsperarExitoAsync("La oferta se registró correctamente.");

        await Page.LlenarFormularioOfertaAsync(proveedorB, 450_000m);
        await Page.EsperarExitoAsync("La oferta se registró correctamente.");

        await Expect(Page.Locator("span.badge").Filter(new() { HasText = "Mejor" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Mejor oferta" }))
            .ToBeVisibleAsync();
        await Expect(Page.Locator(".card.border-success"))
            .ToContainTextAsync(proveedorB);

        await Page.LlenarFormularioOfertaAsync(proveedorA, 400_000m);
        await Page.EsperarErrorAsync(
            "Este proveedor ya tiene una oferta registrada para esta licitación.");
    }

    [Fact]
    public async Task Registrar_OfertaQueSuperaElPresupuesto_EsRechazada()
    {
        var sufijo = TestSettings.SufijoUnico();
        var codigo = TestSettings.CodigoLicitacion(sufijo);
        var proveedor = TestSettings.NombreProveedor(sufijo);

        await Page.CrearProveedorAsync(proveedor);
        await Page.CrearLicitacionAsync(codigo, $"Licitación con presupuesto {sufijo}", 100_000m);
        await Page.AbrirDetalleLicitacionAsync(codigo);
        await Page.PublicarAsync();

        await Page.LlenarFormularioOfertaAsync(proveedor, 200_000m);

        await Page.EsperarErrorAsync("no puede superar el presupuesto estimado");
    }

    [Fact]
    public async Task Cerrar_Publicada_OcultaElFormularioDeNuevasOfertas()
    {
        var sufijo = TestSettings.SufijoUnico();
        var codigo = TestSettings.CodigoLicitacion(sufijo);

        await Page.CrearLicitacionAsync(codigo, $"Licitación con cierre {sufijo}", 100_000m);
        await Page.AbrirDetalleLicitacionAsync(codigo);
        await Page.PublicarAsync();

        await Expect(Page.Locator("#ProveedorId")).ToBeVisibleAsync();

        await Page.CerrarLicitacionAsync("Cierre para impedir nuevas ofertas.");

        await Expect(Page.Locator("#ProveedorId")).ToHaveCountAsync(0);
    }
}
