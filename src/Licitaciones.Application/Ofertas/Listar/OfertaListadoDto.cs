namespace Licitaciones.Application.Ofertas.Listar;

public sealed record OfertaListadoDto(
    Guid Id,
    Guid LicitacionId,
    string CodigoLicitacion,
    Guid ProveedorId,
    string NombreProveedor,
    decimal MontoOfertadoCrc,
    DateTimeOffset FechaRegistro);
