using Licitaciones.Application.Ofertas.Listar;
using Licitaciones.Application.Ofertas.OpcionesFiltro;
using Licitaciones.Web.Models.Ofertas;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

[Route("Ofertas")]
public sealed class OfertasController(
    ListarOfertasHandler listarOfertasHandler,
    OpcionesFiltroOfertasHandler opcionesFiltroHandler) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? licitacionId = null,
        [FromQuery] Guid? proveedorId = null,
        [FromQuery] string? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        var opciones = await opcionesFiltroHandler.HandleAsync(
            new OpcionesFiltroOfertasQuery(),
            cancellationToken);

        var resultado = await listarOfertasHandler.HandleAsync(
            new ListarOfertasQuery(
                page,
                pageSize,
                licitacionId,
                proveedorId,
                sortBy),
            cancellationToken);

        return View(new ListarOfertasViewModel(
            resultado,
            opciones,
            licitacionId,
            proveedorId,
            sortBy ?? "monto",
            resultado.TamanoPagina));
    }
}
