using Licitaciones.Application.Licitaciones.Ports;

namespace Licitaciones.Application.Licitaciones.Detalle;

public static class CalculadorMejorOferta
{
    public static OfertaBasica? Calcular(
        IReadOnlyList<OfertaBasica> ofertas)
    {
        if (ofertas.Count == 0)
        {
            return null;
        }

        return ofertas
            .OrderBy(o => o.MontoOfertadoCrc)
            .ThenBy(o => o.FechaRegistro)
            .First();
    }
}
