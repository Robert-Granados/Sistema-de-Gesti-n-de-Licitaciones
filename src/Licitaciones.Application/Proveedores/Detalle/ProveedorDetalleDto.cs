using Licitaciones.Application.Common.Models;
using Licitaciones.Domain.Enums;

namespace Licitaciones.Application.Proveedores.Detalle;

public sealed record ProveedorDetalleDto(
    Guid Id,
    string Nombre,
    PaginaResultado<OfertaProveedorDto> Ofertas);

public sealed record OfertaProveedorDto(
    Guid Id,
    Guid LicitacionId,
    string CodigoLicitacion,
    string TituloLicitacion,
    decimal MontoOfertadoCrc,
    DateTimeOffset FechaRegistro,
    EstadoLicitacion Estado);

