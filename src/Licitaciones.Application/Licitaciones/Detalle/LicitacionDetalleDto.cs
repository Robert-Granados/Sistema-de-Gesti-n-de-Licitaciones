using Licitaciones.Application.Common.Models;
using Licitaciones.Domain.Enums;

namespace Licitaciones.Application.Licitaciones.Detalle;

public sealed record LicitacionDetalleDto(
    Guid Id,
    string Codigo,
    string Titulo,
    EstadoLicitacion Estado,
    DateTimeOffset FechaCierre,
    decimal PresupuestoEstimadoCrc,
    PaginaResultado<OfertaDetalleDto> Ofertas,
    MejorOfertaInfo? MejorOferta);

public sealed record OfertaDetalleDto(
    Guid Id,
    string NombreProveedor,
    decimal MontoOfertadoCrc,
    DateTimeOffset FechaRegistro);

public sealed record MejorOfertaInfo(
    Guid OfertaId,
    string NombreProveedor,
    decimal MontoOfertadoCrc,
    ClasificacionAhorro Clasificacion,
    string? Aprobador,
    decimal? MontoUsd,
    DateTimeOffset? FechaVigenciaTipoCambio);
