using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.Licitaciones.Ports;

public interface ILicitacionDetalleRepository
{
    Task<LicitacionDetalleCompleta?> ObtenerPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed record LicitacionDetalleCompleta(
    Licitacion Licitacion,
    IReadOnlyList<OfertaBasica> Ofertas,
    IReadOnlyList<NivelAprobacion> NivelesAprobacion,
    TipoCambio? TipoCambioActivo);

public sealed record OfertaBasica(
    Guid Id,
    Guid ProveedorId,
    string NombreProveedor,
    decimal MontoOfertadoCrc,
    DateTimeOffset FechaRegistro);
