using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.Licitaciones.Detalle;

public static class ResolverAprobadorService
{
    public static string? Resolver(
        IReadOnlyList<NivelAprobacion> niveles,
        decimal monto)
    {
        return niveles
            .FirstOrDefault(nivel => nivel.Contiene(monto))
            ?.Aprobador;
    }
}
