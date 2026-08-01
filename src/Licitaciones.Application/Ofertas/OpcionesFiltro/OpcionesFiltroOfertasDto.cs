using Licitaciones.Application.Licitaciones.Detalle;

namespace Licitaciones.Application.Ofertas.OpcionesFiltro;

public sealed record OpcionesFiltroOfertasDto(
    IReadOnlyList<OpcionLicitacionDto> Licitaciones,
    IReadOnlyList<ProveedorBasicoDto> Proveedores);

public sealed record OpcionLicitacionDto(Guid Id, string Codigo);
