using Licitaciones.Api.Contracts;
using Licitaciones.Application.TiposCambio;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Licitaciones.Api.Controllers;

[ApiController]
[Route("api/v1/tiposcambio")]
public sealed class ApiTiposCambioController(TipoCambioService service) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Lista tipos de cambio", Description = "Admite page, pageSize, activo y sortBy=fecha|fecha_asc|valor.")]
    public async Task<ActionResult<PaginaApi<TipoCambioDto>>> Listar(
        int page = 1, int pageSize = 20, bool? activo = null,
        string? sortBy = null, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = (await service.ListarAsync(cancellationToken)).AsEnumerable();
        if (activo.HasValue) query = query.Where(t => t.Activo == activo.Value);
        query = sortBy switch
        {
            "fecha_asc" => query.OrderBy(t => t.FechaVigencia),
            "valor" => query.OrderBy(t => t.CrcPorUsd),
            _ => query.OrderByDescending(t => t.FechaVigencia)
        };
        var all = query.ToList();
        return Ok(new PaginaApi<TipoCambioDto>(
            all.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            all.Count, page, pageSize, (int)Math.Ceiling(all.Count / (double)pageSize)));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TipoCambioDto>> Obtener(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.ObtenerAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Crea un tipo de cambio", Description = "Ejemplo: { \"crcPorUsd\":520.25, \"fechaVigencia\":\"2026-08-05T00:00:00-06:00\", \"activar\":true }")]
    public async Task<ActionResult<TipoCambioDto>> Crear(
        GuardarTipoCambioRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CrearAsync(
            request.CrcPorUsd, request.FechaVigencia, request.Activar, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TipoCambioDto>> Editar(
        Guid id, GuardarTipoCambioRequest request, CancellationToken cancellationToken)
    {
        var result = await service.EditarAsync(
            id, request.CrcPorUsd, request.FechaVigencia, cancellationToken);
        if (request.Activar) result = await service.ActivarAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/activar")]
    [SwaggerOperation(Summary = "Activa un tipo de cambio y desactiva el anterior")]
    public async Task<ActionResult<TipoCambioDto>> Activar(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.ActivarAsync(id, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        await service.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}
