using Licitaciones.Domain.Entities;
using Licitaciones.Application.NivelesAprobacion;

namespace Licitaciones.Application.Licitaciones.Detalle;

public sealed class ResolverAprobadorService(INivelAprobacionRepository repository)
{
    public const string SinAprobadorConfigurado = "Sin aprobador configurado";

    public async Task<ResultadoAprobador> Resolver(
        decimal monto,
        CancellationToken cancellationToken = default)
    {
        var niveles = await repository.ListarOrdenadosAsync(cancellationToken);
        var nivel = niveles.FirstOrDefault(n => n.Contiene(monto));
        return nivel is null
            ? new ResultadoAprobador(false, SinAprobadorConfigurado, null)
            : new ResultadoAprobador(true, nivel.Aprobador, nivel.Id);
    }

    public static string? Resolver(
        IReadOnlyList<NivelAprobacion> niveles,
        decimal monto)
    {
        return niveles
            .OrderBy(nivel => nivel.MontoMinimoCrc)
            .FirstOrDefault(nivel => nivel.Contiene(monto))
            ?.Aprobador
            ?? SinAprobadorConfigurado;
    }
}

public sealed record ResultadoAprobador(
    bool Configurado,
    string Aprobador,
    Guid? NivelAprobacionId);
