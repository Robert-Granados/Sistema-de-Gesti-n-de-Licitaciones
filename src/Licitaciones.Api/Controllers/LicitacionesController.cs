using Licitaciones.Api.Contracts;
using Licitaciones.Application.Licitaciones.Cerrar;
using Licitaciones.Application.Licitaciones.Crear;
using Licitaciones.Application.Licitaciones.Detalle;
using Licitaciones.Application.Licitaciones.Editar;
using Licitaciones.Application.Licitaciones.Eliminar;
using Licitaciones.Application.Licitaciones.Listar;
using Licitaciones.Application.Licitaciones.Publicar;
using Licitaciones.Application.Licitaciones.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Licitaciones.Api.Controllers;

[ApiController]
[Route("api/v1/licitaciones")]
public sealed class ApiLicitacionesController(
    CrearLicitacionHandler crear,
    ListarLicitacionesHandler listar,
    ObtenerLicitacionPorIdHandler detalle,
    EditarLicitacionHandler editar,
    PublicarLicitacionHandler publicar,
    CerrarLicitacionHandler cerrar,
    EliminarLicitacionHandler eliminar) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Lista licitaciones", Description = "Admite page, pageSize, search, estado, fechaDesde, fechaHasta y sortBy.")]
    public Task<Application.Common.Models.PaginaResultado<LicitacionListadoDto>> Listar(
        int page = 1, int pageSize = 20, string? search = null, string? estado = null,
        DateTimeOffset? fechaDesde = null, DateTimeOffset? fechaHasta = null,
        string? sortBy = null, CancellationToken cancellationToken = default) =>
        listar.HandleAsync(new(page, pageSize, search, estado, fechaDesde, fechaHasta, sortBy), cancellationToken);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LicitacionApiResponse>> Obtener(Guid id, CancellationToken cancellationToken)
    {
        var result = await detalle.HandleAsync(new(id), cancellationToken);
        var version = await editar.ObtenerAsync(id, cancellationToken);
        return result is null || version is null
            ? NotFound()
            : Ok(new LicitacionApiResponse(result, version.RowVersion));
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Crea una licitación", Description = "Ejemplo: { \"codigo\":\"LIC-2026-001\", \"titulo\":\"Equipo\", \"fechaCierre\":\"2026-09-01T18:00:00Z\", \"presupuestoEstimadoCrc\":1000000 }")]
    public async Task<ActionResult<CrearLicitacionResult>> Crear(
        CrearLicitacionRequest request, CancellationToken cancellationToken)
    {
        var result = await crear.HandleAsync(
            new(request.Codigo, request.Titulo, request.FechaCierre, request.PresupuestoEstimadoCrc),
            cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Editar(
        Guid id, EditarLicitacionRequest request, CancellationToken cancellationToken)
    {
        await editar.HandleAsync(
            new(id, request.Titulo, request.FechaCierre, request.PresupuestoEstimadoCrc, request.RowVersion),
            cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        try { await eliminar.HandleAsync(new(id), cancellationToken); return NoContent(); }
        catch (LicitacionNoEncontradaException) { return NotFound(); }
    }

    [HttpPost("{id:guid}/publicar")]
    [SwaggerOperation(Summary = "Publica una licitación")]
    public async Task<IActionResult> Publicar(Guid id, CancellationToken cancellationToken)
    {
        await publicar.HandleAsync(new(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/cerrar")]
    [SwaggerOperation(Summary = "Cierra una licitación", Description = "Ejemplo: { \"motivo\":\"Proceso adjudicado\" }")]
    public async Task<IActionResult> Cerrar(
        Guid id, CerrarLicitacionRequest request, CancellationToken cancellationToken)
    {
        await cerrar.HandleAsync(new(id, request.Motivo), cancellationToken);
        return NoContent();
    }
}
