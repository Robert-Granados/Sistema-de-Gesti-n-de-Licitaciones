using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Proveedores.Listar;

namespace Licitaciones.Application.Proveedores.Ports;

public interface IProveedorReadRepository
{
    Task<PaginaResultado<ProveedorListadoDto>> ListarAsync(
        ProveedoresConsulta consulta,
        CancellationToken cancellationToken = default);
}

public sealed record ProveedoresConsulta(
    int Page,
    int PageSize,
    string? Search,
    OrdenProveedor SortBy);

public enum OrdenProveedor
{
    NombreAscendente,
    NombreDescendente,
    IdAscendente,
    IdDescendente
}

