using Licitaciones.Application.TiposCambio;
using Licitaciones.Web.Models.TiposCambio;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

[Route("TiposCambio")]
public sealed class TiposCambioController(TipoCambioService service) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var tiposCambio = await service.ListarAsync(cancellationToken);
        return View(tiposCambio);
    }

    [HttpGet("Crear")]
    public IActionResult Crear() => View(new CrearTipoCambioViewModel());

    [HttpPost("Crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(
        CrearTipoCambioViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var tipoCambio = await service.CrearAsync(
                model.CrcPorUsd,
                model.FechaVigencia!.Value,
                model.Activar,
                cancellationToken);

            TempData["MensajeExito"] = tipoCambio.Activo
                ? "El tipo de cambio se registró y quedó activo."
                : "El tipo de cambio se registró correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            TempData["MensajeError"] = exception.Message;
            ModelState.AddModelError(
                nameof(model.CrcPorUsd),
                exception.Message);
            return View(model);
        }
    }

    [HttpGet("Editar/{id:guid}")]
    public async Task<IActionResult> Editar(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tipoCambio = await service.ObtenerAsync(id, cancellationToken);

        return tipoCambio is null
            ? NotFound()
            : View(new EditarTipoCambioViewModel
            {
                Id = tipoCambio.Id,
                CrcPorUsd = tipoCambio.CrcPorUsd,
                FechaVigencia = tipoCambio.FechaVigencia,
                Activo = tipoCambio.Activo
            });
    }

    [HttpPost("Editar/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        Guid id,
        EditarTipoCambioViewModel model,
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
            await service.EditarAsync(
                model.Id,
                model.CrcPorUsd,
                model.FechaVigencia!.Value,
                cancellationToken);

            TempData["MensajeExito"] =
                "El tipo de cambio se actualizó correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            TempData["MensajeError"] = exception.Message;
            ModelState.AddModelError(
                nameof(model.CrcPorUsd),
                exception.Message);
            return View(model);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("Activar/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activar(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await service.ActivarAsync(id, cancellationToken);
            TempData["MensajeExito"] =
                "El tipo de cambio se activó y ahora es el vigente.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Eliminar/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await service.EliminarAsync(id, cancellationToken);
            TempData["MensajeExito"] =
                "El tipo de cambio se eliminó correctamente.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}
