namespace Licitaciones.Application.Proveedores.Editar;

public sealed record EditarProveedorDto(
    Guid Id,
    string Nombre,
    int RowVersion);

