namespace Licitaciones.Application.Ofertas.Editar;

public sealed record EditarOfertaDto(
    Guid Id,
    Guid LicitacionId,
    string CodigoLicitacion,
    string NombreProveedor,
    decimal MontoOfertadoCrc);
