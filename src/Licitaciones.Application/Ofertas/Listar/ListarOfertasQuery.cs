namespace Licitaciones.Application.Ofertas.Listar;

public sealed record ListarOfertasQuery(
    int Page = 1,
    int PageSize = 10,
    Guid? LicitacionId = null,
    Guid? ProveedorId = null,
    string? SortBy = null);
