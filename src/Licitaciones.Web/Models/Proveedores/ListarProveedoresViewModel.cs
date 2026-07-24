using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Proveedores.Listar;

namespace Licitaciones.Web.Models.Proveedores;

public sealed record ListarProveedoresViewModel(
    PaginaResultado<ProveedorListadoDto> Resultado,
    string? Search,
    string SortBy,
    int PageSize);

