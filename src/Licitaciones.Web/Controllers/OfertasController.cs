using Licitaciones.Application.Ofertas.Editar;
using Licitaciones.Application.Ofertas.Eliminar;
using Licitaciones.Application.Ofertas.Exceptions;
using Licitaciones.Application.Ofertas.Listar;
using Licitaciones.Application.Ofertas.OpcionesFiltro;
using Licitaciones.Web.Models.Ofertas;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

[Route("Ofertas")]
public sealed class OfertasController(
    ListarOfertasHandler listarOfertasHandler,
    OpcionesFiltroOfertasHandler opcionesFiltroHandler,
    EditarOfertaHandler editarOfertaHandler,
    EliminarOfertaHandler eliminarOfertaHandler) : Controller
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

    [HttpGet("Editar/{id:guid}")]
    public async Task<IActionResult> Editar(
        Guid id,
        CancellationToken cancellationToken)
    {
        var oferta = await editarOfertaHandler.ObtenerAsync(
            id,
            cancellationToken);

        if (oferta is null)
        {
            return NotFound();
        }

        return View(new EditarOfertaViewModel
        {
            Id = oferta.Id,
            LicitacionId = oferta.LicitacionId,
            CodigoLicitacion = oferta.CodigoLicitacion,
            NombreProveedor = oferta.NombreProveedor,
            MontoOfertadoCrc = oferta.MontoOfertadoCrc
        });
    }

    [HttpPost("Editar/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        Guid id,
        EditarOfertaViewModel model,
        CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await editarOfertaHandler.HandleAsync(
                new EditarOfertaCommand(
                    model.Id,
                    model.MontoOfertadoCrc),
                cancellationToken);

            TempData["MensajeExito"] = "La oferta se actualizó correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (OfertaNoEncontradaException)
        {
            return NotFound();
        }
        catch (LicitacionNoDisponibleException exception)
        {
            TempData["MensajeError"] = exception.Message;
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            TempData["MensajeError"] = exception.Message;
            ModelState.AddModelError(
                nameof(model.MontoOfertadoCrc),
                exception.Message);
            return View(model);
        }
    }

    [HttpPost("Eliminar/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await eliminarOfertaHandler.HandleAsync(
                new EliminarOfertaCommand(id),
                cancellationToken);

            TempData["MensajeExito"] = "La oferta se eliminó correctamente.";
        }
        catch (OfertaNoEncontradaException)
        {
            return NotFound();
        }
        catch (LicitacionNoDisponibleException exception)
        {
            TempData["MensajeError"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
