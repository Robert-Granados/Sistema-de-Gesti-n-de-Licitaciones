using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Ofertas.Listar;

namespace Licitaciones.Application.Ofertas.Ports;

public interface IOfertaReadRepository
{
    Task<PaginaResultado<OfertaListadoDto>> ListarAsync(
        OfertasConsulta consulta,
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
