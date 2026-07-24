using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Licitaciones.Crear;
using Licitaciones.Application.Licitaciones.Exceptions;
using Licitaciones.Application.Licitaciones.Listar;
using Licitaciones.Web.Models.Licitaciones;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

[Route("Licitaciones")]
public sealed class LicitacionesController(
    CrearLicitacionHandler crearLicitacionHandler,
    ListarLicitacionesHandler listarLicitacionesHandler,
    IClock clock) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? filtroEstado = null,
        [FromQuery] string? fechaDesde = null,
        [FromQuery] string? fechaHasta = null,
        [FromQuery] string? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset? desde = null;
        DateTimeOffset? hasta = null;

        if (DateTimeOffset.TryParse(fechaDesde, out var parsedDesde))
        {
            desde = parsedDesde;
        }

        if (DateTimeOffset.TryParse(fechaHasta, out var parsedHasta))
        {
            hasta = parsedHasta;
        }

        var resultado = await listarLicitacionesHandler.HandleAsync(
            new ListarLicitacionesQuery(
                page,
                pageSize,
                search,
                filtroEstado,
                desde,
                hasta,
                sortBy),
            cancellationToken);

        return View(new ListarLicitacionesViewModel(
            resultado,
            search,
            filtroEstado,
            fechaDesde,
            fechaHasta,
            sortBy ?? "fecha_cierre",
            resultado.TamanoPagina));
    }

    [HttpGet("Crear")]
    public IActionResult Crear()
    {
        var model = new CrearLicitacionViewModel
        {
            FechaCierre = clock.UtcNow
                .ToOffset(TimeSpan.FromHours(-6))
                .AddDays(15)
        };

        return View(model);
    }

    [HttpPost("Crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(
        CrearLicitacionViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.FechaCierre!.Value <= clock.UtcNow)
        {
            ModelState.AddModelError(
                nameof(model.FechaCierre),
                "La fecha de cierre debe ser futura.");
            return View(model);
        }

        try
        {
            var result = await crearLicitacionHandler.HandleAsync(
                new CrearLicitacionCommand(
                    model.Codigo,
                    model.Titulo,
                    model.FechaCierre.Value,
                    model.PresupuestoEstimadoCrc),
                cancellationToken);

            TempData["MensajeExito"] =
                $"La licitación \"{result.Codigo}\" se registró correctamente.";

            return RedirectToAction(nameof(Crear));
        }
        catch (LicitacionDuplicadaException exception)
        {
            ModelState.AddModelError(
                nameof(model.Codigo), exception.Message);
            Response.StatusCode = StatusCodes.Status409Conflict;
            return View(model);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }
}
