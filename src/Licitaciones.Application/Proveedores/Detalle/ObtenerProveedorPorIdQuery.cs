namespace Licitaciones.Application.Proveedores.Detalle;

public sealed record ObtenerProveedorPorIdQuery(
    Guid Id,
    int Page = 1,
    int PageSize = 10);

