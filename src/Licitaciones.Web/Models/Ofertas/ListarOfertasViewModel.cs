using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Ofertas.Listar;
using Licitaciones.Application.Ofertas.OpcionesFiltro;

namespace Licitaciones.Web.Models.Ofertas;

public sealed record ListarOfertasViewModel(
    PaginaResultado<OfertaListadoDto> Resultado,
    OpcionesFiltroOfertasDto OpcionesFiltro,
    Guid? LicitacionId,
    Guid? ProveedorId,
    string SortBy,
    int PageSize);
