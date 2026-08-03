using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Licitaciones.Ports;

namespace Licitaciones.Application.Licitaciones.Detalle;

public sealed class ObtenerLicitacionPorIdHandler(
    ILicitacionDetalleRepository repository)
{
    private const int PageSizeDefault = 100;
    private const int PageSizeMaximum = 500;

    public async Task<LicitacionDetalleDto?> HandleAsync(
        ObtenerLicitacionPorIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Id == Guid.Empty)
        {
            return null;
        }

        var detalle = await repository.ObtenerPorIdAsync(
            query.Id,
            cancellationToken);

        if (detalle is null)
        {
            return null;
        }

        var mejorOferta = CalculadorMejorOferta.Calcular(detalle.Ofertas);

        var mejorOfertaInfo = mejorOferta is not null
            ? new MejorOfertaInfo(
                mejorOferta.Id,
                mejorOferta.NombreProveedor,
                mejorOferta.MontoOfertadoCrc,
                ClasificadorAhorro.Clasificar(
                    detalle.Licitacion.PresupuestoEstimadoCrc,
                    mejorOferta.MontoOfertadoCrc),
                ResolverAprobadorService.Resolver(
                    detalle.NivelesAprobacion,
                    mejorOferta.MontoOfertadoCrc),
                ConversionMonedaService.ConvertirAUsd(
                    detalle.TipoCambioActivo,
                    mejorOferta.MontoOfertadoCrc)?.MontoUsd,
                ConversionMonedaService.ConvertirAUsd(
                    detalle.TipoCambioActivo,
                    mejorOferta.MontoOfertadoCrc)?.FechaVigencia)
            : null;

        var ofertasDto = detalle.Ofertas
            .OrderBy(o => o.MontoOfertadoCrc)
            .ThenBy(o => o.FechaRegistro)
            .Select(o => new OfertaDetalleDto(
                o.Id,
                o.NombreProveedor,
                o.MontoOfertadoCrc,
                o.FechaRegistro))
            .ToList();

        var proveedoresDto = detalle.ProveedoresDisponibles
            .Select(p => new ProveedorBasicoDto(p.Id, p.Nombre))
            .ToList();

        return new LicitacionDetalleDto(
            detalle.Licitacion.Id,
            detalle.Licitacion.Codigo,
            detalle.Licitacion.Titulo,
            detalle.Licitacion.Estado,
            detalle.Licitacion.FechaCierre,
            detalle.Licitacion.PresupuestoEstimadoCrc,
            new PaginaResultado<OfertaDetalleDto>(
                ofertasDto,
                ofertasDto.Count,
                1,
                PageSizeMaximum),
            mejorOfertaInfo,
            proveedoresDto,
            detalle.TipoCambioActivo is null
                ? null
                : new TipoCambioVisualizacionDto(
                    detalle.TipoCambioActivo.CrcPorUsd,
                    detalle.TipoCambioActivo.FechaVigencia));
    }
}
