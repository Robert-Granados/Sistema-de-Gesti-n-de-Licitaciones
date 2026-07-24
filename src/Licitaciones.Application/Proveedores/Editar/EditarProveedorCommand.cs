namespace Licitaciones.Application.Proveedores.Editar;

public sealed record EditarProveedorCommand(
    Guid Id,
    string Nombre,
    int RowVersion);

