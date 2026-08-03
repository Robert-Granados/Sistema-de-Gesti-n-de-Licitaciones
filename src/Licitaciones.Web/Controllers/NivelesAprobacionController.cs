using Licitaciones.Application.NivelesAprobacion;
using Licitaciones.Web.Models.NivelesAprobacion;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

[Route("NivelesAprobacion")]
public sealed class NivelesAprobacionController(NivelAprobacionService service) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var niveles = await service.ListarAsync(cancellationToken);
        return View(niveles);
    }

    [HttpGet("Crear")]
    public IActionResult Crear() => View(new CrearNivelAprobacionViewModel());

    [HttpPost("Crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(
        CrearNivelAprobacionViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var nivel = await service.CrearAsync(
                model.MontoMinimoCrc,
                model.MontoMaximoCrc,
                model.Aprobador,
                cancellationToken);

            TempData["MensajeExito"] =
                $"El nivel de aprobación para “{nivel.Aprobador}” se registró correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (NivelAprobacionException exception)
        {
            TempData["MensajeError"] = exception.Message;
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            TempData["MensajeError"] = exception.Message;
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [HttpGet("Editar/{id:guid}")]
    public async Task<IActionResult> Editar(
        Guid id,
        CancellationToken cancellationToken)
    {
        var nivel = await service.ObtenerAsync(id, cancellationToken);

        return nivel is null
            ? NotFound()
            : View(new EditarNivelAprobacionViewModel
            {
                Id = nivel.Id,
                MontoMinimoCrc = nivel.MontoMinimoCrc,
                MontoMaximoCrc = nivel.MontoMaximoCrc,
                Aprobador = nivel.Aprobador
            });
    }

    [HttpPost("Editar/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        Guid id,
        EditarNivelAprobacionViewModel model,
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
                model.MontoMinimoCrc,
                model.MontoMaximoCrc,
                model.Aprobador,
                cancellationToken);

            TempData["MensajeExito"] =
                "El nivel de aprobación se actualizó correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (NivelAprobacionException exception)
        {
            TempData["MensajeError"] = exception.Message;
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            TempData["MensajeError"] = exception.Message;
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
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
            await service.EliminarAsync(id, cancellationToken);
            TempData["MensajeExito"] =
                "El nivel de aprobación se eliminó correctamente.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}
