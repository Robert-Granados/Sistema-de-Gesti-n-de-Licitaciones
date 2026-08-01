namespace Licitaciones.Application.Licitaciones.Detalle;

public static class ClasificadorAhorro
{
    public static ClasificacionAhorro Clasificar(
        decimal presupuestoEstimadoCrc,
        decimal montoMejorOfertaCrc)
    {
        if (montoMejorOfertaCrc >= presupuestoEstimadoCrc)
        {
            return ClasificacionAhorro.OfertaValidaSinAhorro;
        }

        var ahorro = (presupuestoEstimadoCrc - montoMejorOfertaCrc)
            / presupuestoEstimadoCrc;

        return ahorro >= 0.10m
            ? ClasificacionAhorro.OfertaConveniente
            : ClasificacionAhorro.OfertaAceptable;
    }
}
