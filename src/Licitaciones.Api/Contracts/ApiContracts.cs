using System.ComponentModel.DataAnnotations;
using Licitaciones.Application.Licitaciones.Detalle;
using Licitaciones.Application.Proveedores.Detalle;

namespace Licitaciones.Api.Contracts;

public sealed record CrearProveedorRequest(
    [property: Required, StringLength(200)] string Nombre);
public sealed record EditarProveedorRequest(
    [property: Required, StringLength(200)] string Nombre,
    int RowVersion);

public sealed record CrearLicitacionRequest(
    [property: Required] string Codigo,
    [property: Required] string Titulo,
    DateTimeOffset FechaCierre,
    decimal PresupuestoEstimadoCrc);
public sealed record EditarLicitacionRequest(
    [property: Required] string Titulo,
    DateTimeOffset FechaCierre,
    decimal PresupuestoEstimadoCrc,
    int RowVersion);
public sealed record CerrarLicitacionRequest(
    [property: Required, StringLength(500)] string Motivo);

public sealed record CrearOfertaRequest(
    Guid LicitacionId,
    Guid ProveedorId,
    decimal MontoOfertadoCrc);
public sealed record EditarOfertaRequest(decimal MontoOfertadoCrc);

public sealed record GuardarNivelAprobacionRequest(
    decimal MontoMinimoCrc,
    decimal? MontoMaximoCrc,
    [property: Required, StringLength(150)] string Aprobador);

public sealed record GuardarTipoCambioRequest(
    decimal CrcPorUsd,
    DateTimeOffset FechaVigencia,
    bool Activar = false);

public sealed record PaginaApi<T>(
    IReadOnlyList<T> Elementos,
    int TotalRegistros,
    int PaginaActual,
    int TamanoPagina,
    int TotalPaginas);

public sealed record RecursoCreadoResponse(Guid Id);

public sealed record ProveedorApiResponse(
    ProveedorDetalleDto Detalle,
    int RowVersion);

public sealed record LicitacionApiResponse(
    LicitacionDetalleDto Detalle,
    int RowVersion);
