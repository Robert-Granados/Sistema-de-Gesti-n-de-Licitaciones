using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Ofertas.Editar;
using Licitaciones.Application.Ofertas.Listar;
using Licitaciones.Application.Ofertas.OpcionesFiltro;

namespace Licitaciones.Application.Ofertas.Ports;

public interface IOfertaReadRepository
{
    Task<PaginaResultado<OfertaListadoDto>> ListarAsync(
        OfertasConsulta consulta,
        CancellationToken cancellationToken = default);

    Task<OpcionesFiltroOfertasDto> ObtenerOpcionesFiltroAsync(
        CancellationToken cancellationToken = default);

    Task<EditarOfertaDto?> ObtenerParaEdicionAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed record OfertasConsulta(
    int Page,
    int PageSize,
    Guid? LicitacionId,
    Guid? ProveedorId,
    OrdenOferta SortBy);

public enum OrdenOferta
{
    MontoAscendente,
    MontoDescendente,
    FechaAscendente,
    FechaDescendente
}
