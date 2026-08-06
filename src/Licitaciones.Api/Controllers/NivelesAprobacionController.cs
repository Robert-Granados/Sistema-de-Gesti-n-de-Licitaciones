using Licitaciones.Api.Contracts;
using Licitaciones.Application.NivelesAprobacion;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Licitaciones.Api.Controllers;

[ApiController]
[Route("api/v1/nivelesaprobacion")]
public sealed class ApiNivelesAprobacionController(NivelAprobacionService service) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Lista niveles de aprobación", Description = "Admite page, pageSize, search y sortBy=minimo|minimo_desc|aprobador.")]
    public async Task<ActionResult<PaginaApi<NivelAprobacionDto>>> Listar(
        int page = 1, int pageSize = 20, string? search = null,
        string? sortBy = null, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = (await service.ListarAsync(cancellationToken)).AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(n => n.Aprobador.Contains(search, StringComparison.OrdinalIgnoreCase));
        query = sortBy switch
        {
            "minimo_desc" => query.OrderByDescending(n => n.MontoMinimoCrc),
            "aprobador" => query.OrderBy(n => n.Aprobador),
            _ => query.OrderBy(n => n.MontoMinimoCrc)
        };
        var all = query.ToList();
        return Ok(new PaginaApi<NivelAprobacionDto>(
            all.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            all.Count, page, pageSize, (int)Math.Ceiling(all.Count / (double)pageSize)));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NivelAprobacionDto>> Obtener(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.ObtenerAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Crea un nivel", Description = "Ejemplo: { \"montoMinimoCrc\":1000, \"montoMaximoCrc\":9999.99, \"aprobador\":\"Gerencia\" }")]
    public async Task<ActionResult<NivelAprobacionDto>> Crear(
        GuardarNivelAprobacionRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CrearAsync(
            request.MontoMinimoCrc, request.MontoMaximoCrc, request.Aprobador, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NivelAprobacionDto>> Editar(
        Guid id, GuardarNivelAprobacionRequest request, CancellationToken cancellationToken) =>
        Ok(await service.EditarAsync(
            id, request.MontoMinimoCrc, request.MontoMaximoCrc, request.Aprobador, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        await service.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}
