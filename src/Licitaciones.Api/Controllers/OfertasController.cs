using Licitaciones.Api.Contracts;
using Licitaciones.Application.Ofertas.Editar;
using Licitaciones.Application.Ofertas.Eliminar;
using Licitaciones.Application.Ofertas.Exceptions;
using Licitaciones.Application.Ofertas.Listar;
using Licitaciones.Application.Ofertas.Registrar;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Licitaciones.Api.Controllers;

[ApiController]
[Route("api/v1/ofertas")]
public sealed class ApiOfertasController(
    RegistrarOfertaHandler crear,
    ListarOfertasHandler listar,
    EditarOfertaHandler editar,
    EliminarOfertaHandler eliminar) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Lista ofertas", Description = "Admite page, pageSize, licitacionId, proveedorId y sortBy.")]
    public Task<Application.Common.Models.PaginaResultado<OfertaListadoDto>> Listar(
        int page = 1, int pageSize = 20, Guid? licitacionId = null,
        Guid? proveedorId = null, string? sortBy = null,
        CancellationToken cancellationToken = default) =>
        listar.HandleAsync(new(page, pageSize, licitacionId, proveedorId, sortBy), cancellationToken);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EditarOfertaDto>> Obtener(Guid id, CancellationToken cancellationToken)
    {
        var result = await editar.ObtenerAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Registra una oferta", Description = "Ejemplo: { \"licitacionId\":\"...\", \"proveedorId\":\"...\", \"montoOfertadoCrc\":750000 }")]
    public async Task<ActionResult<RegistrarOfertaResult>> Crear(
        CrearOfertaRequest request, CancellationToken cancellationToken)
    {
        var result = await crear.HandleAsync(
            new(request.LicitacionId, request.ProveedorId, request.MontoOfertadoCrc),
            cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Editar(
        Guid id, EditarOfertaRequest request, CancellationToken cancellationToken)
    {
        await editar.HandleAsync(new(id, request.MontoOfertadoCrc), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        try { await eliminar.HandleAsync(new(id), cancellationToken); return NoContent(); }
        catch (OfertaNoEncontradaException) { return NotFound(); }
    }
}
