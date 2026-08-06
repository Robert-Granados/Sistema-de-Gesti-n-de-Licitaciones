using Licitaciones.Api.Contracts;
using Licitaciones.Application.Proveedores.Crear;
using Licitaciones.Application.Proveedores.Detalle;
using Licitaciones.Application.Proveedores.Editar;
using Licitaciones.Application.Proveedores.Eliminar;
using Licitaciones.Application.Proveedores.Listar;
using Licitaciones.Application.Proveedores.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Licitaciones.Api.Controllers;

[ApiController]
[Route("api/v1/proveedores")]
public sealed class ApiProveedoresController(
    CrearProveedorHandler crear,
    ListarProveedoresHandler listar,
    ObtenerProveedorPorIdHandler detalle,
    EditarProveedorHandler editar,
    EliminarProveedorHandler eliminar) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Lista proveedores", Description = "Ejemplo: GET /api/v1/proveedores?page=1&pageSize=20&search=acme&sortBy=nombre")]
    public Task<Application.Common.Models.PaginaResultado<ProveedorListadoDto>> Listar(
        int page = 1, int pageSize = 20, string? search = null, string? sortBy = null,
        CancellationToken cancellationToken = default) =>
        listar.HandleAsync(new(page, pageSize, search, sortBy), cancellationToken);

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Obtiene un proveedor por ID")]
    public async Task<ActionResult<ProveedorApiResponse>> Obtener(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await detalle.HandleAsync(new(id, 1, 100), cancellationToken);
        var version = await editar.ObtenerAsync(id, cancellationToken);
        return result is null || version is null
            ? NotFound()
            : Ok(new ProveedorApiResponse(result, version.RowVersion));
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Crea un proveedor", Description = "Ejemplo: { \"nombre\": \"Suministros del Valle\" }")]
    public async Task<ActionResult<CrearProveedorResult>> Crear(
        CrearProveedorRequest request, CancellationToken cancellationToken)
    {
        var result = await crear.HandleAsync(new(request.Nombre), cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Actualiza un proveedor")]
    public async Task<IActionResult> Editar(
        Guid id, EditarProveedorRequest request, CancellationToken cancellationToken)
    {
        await editar.HandleAsync(new(id, request.Nombre, request.RowVersion), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Elimina lógicamente un proveedor")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await eliminar.HandleAsync(new(id), cancellationToken);
            return NoContent();
        }
        catch (ProveedorNoEncontradoException) { return NotFound(); }
    }
}
