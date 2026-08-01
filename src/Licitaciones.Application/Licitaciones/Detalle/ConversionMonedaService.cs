using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.Licitaciones.Detalle;

public static class ConversionMonedaService
{
    public static (decimal MontoUsd, DateTimeOffset FechaVigencia)? ConvertirAUsd(
        TipoCambio? tipoCambioActivo,
        decimal montoCrc)
    {
        if (tipoCambioActivo is null || tipoCambioActivo.CrcPorUsd <= 0)
        {
            return null;
        }

        var montoUsd = Math.Round(montoCrc / tipoCambioActivo.CrcPorUsd, 2);
        return (montoUsd, tipoCambioActivo.FechaVigencia);
    }
}
