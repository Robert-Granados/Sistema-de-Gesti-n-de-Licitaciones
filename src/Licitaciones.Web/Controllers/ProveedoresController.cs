using Licitaciones.Application.Proveedores.Crear;
using Licitaciones.Application.Proveedores.Exceptions;
using Licitaciones.Application.Proveedores.Listar;
using Licitaciones.Application.Proveedores.Detalle;
using Licitaciones.Application.Proveedores.Editar;
using Licitaciones.Application.Proveedores.Eliminar;
using Licitaciones.Web.Models.Proveedores;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

[Route("Proveedores")]
public sealed class ProveedoresController(
    CrearProveedorHandler crearProveedorHandler,
    ListarProveedoresHandler listarProveedoresHandler,
    ObtenerProveedorPorIdHandler obtenerProveedorPorIdHandler,
    EditarProveedorHandler editarProveedorHandler,
    EliminarProveedorHandler eliminarProveedorHandler) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        var resultado = await listarProveedoresHandler.HandleAsync(
            new ListarProveedoresQuery(page, pageSize, search, sortBy),
            cancellationToken);

        return View(new ListarProveedoresViewModel(
            resultado,
            search,
            sortBy ?? "nombre",
            resultado.TamanoPagina));
    }

    [HttpGet("Editar/{id:guid}")]
    public async Task<IActionResult> Editar(
        Guid id,
        CancellationToken cancellationToken)
    {
        var proveedor = await editarProveedorHandler.ObtenerAsync(
            id,
            cancellationToken);

        return proveedor is null
            ? NotFound()
            : View(new EditarProveedorViewModel
            {
                Id = proveedor.Id,
                Nombre = proveedor.Nombre,
                RowVersion = proveedor.RowVersion
            });
    }

    [HttpPost("Editar/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        Guid id,
        EditarProveedorViewModel model,
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
            await editarProveedorHandler.HandleAsync(
                new EditarProveedorCommand(
                    model.Id,
                    model.Nombre,
                    model.RowVersion),
                cancellationToken);

            TempData["MensajeExito"] =
                "El proveedor se actualizó correctamente.";

            return RedirectToAction(nameof(Detalle), new { id = model.Id });
        }
        catch (ProveedorNoEncontradoException)
        {
            return NotFound();
        }
        catch (NombreProveedorInvalidoException exception)
        {
            TempData["MensajeError"] = exception.Message;
            ModelState.AddModelError(nameof(model.Nombre), exception.Message);
            return View(model);
        }
        catch (ProveedorDuplicadoException exception)
        {
            TempData["MensajeError"] = exception.Message;
            ModelState.AddModelError(nameof(model.Nombre), exception.Message);
            Response.StatusCode = StatusCodes.Status409Conflict;
            return View(model);
        }
        catch (ProveedorConcurrenciaException exception)
        {
            TempData["MensajeError"] = exception.Message;
            ModelState.AddModelError(string.Empty, exception.Message);
            Response.StatusCode = StatusCodes.Status409Conflict;
            return View(model);
        }
    }

    [HttpGet("Detalle/{id:guid}")]
    public async Task<IActionResult> Detalle(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var proveedor = await obtenerProveedorPorIdHandler.HandleAsync(
            new ObtenerProveedorPorIdQuery(id, page, pageSize),
            cancellationToken);

        return proveedor is null
            ? NotFound()
            : View(proveedor);
    }

    [HttpPost("Eliminar/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await eliminarProveedorHandler.HandleAsync(
                new EliminarProveedorCommand(id),
                cancellationToken);

            TempData["MensajeExito"] = resultado.TeniaOfertas
                ? "El proveedor se eliminó lógicamente y se conservó su historial de ofertas."
                : "El proveedor se eliminó lógicamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (ProveedorNoEncontradoException)
        {
            return NotFound();
        }
    }

    [HttpGet("Crear")]
    public IActionResult Crear() => View(new CrearProveedorViewModel());

    [HttpPost("Crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(
        CrearProveedorViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await crearProveedorHandler.HandleAsync(
                new CrearProveedorCommand(model.Nombre),
                cancellationToken);

            TempData["MensajeExito"] =
                $"El proveedor “{result.Nombre}” se registró correctamente.";

            return RedirectToAction(nameof(Crear));
        }
        catch (NombreProveedorInvalidoException exception)
        {
            TempData["MensajeError"] = exception.Message;
            ModelState.AddModelError(nameof(model.Nombre), exception.Message);
            return View(model);
        }
        catch (ProveedorDuplicadoException exception)
        {
            TempData["MensajeError"] = exception.Message;
            ModelState.AddModelError(nameof(model.Nombre), exception.Message);
            Response.StatusCode = StatusCodes.Status409Conflict;
            return View(model);
        }
    }
}
