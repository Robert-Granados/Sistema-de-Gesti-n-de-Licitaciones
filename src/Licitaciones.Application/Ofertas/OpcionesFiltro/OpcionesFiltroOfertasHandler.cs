using Licitaciones.Application.Ofertas.Ports;

namespace Licitaciones.Application.Ofertas.OpcionesFiltro;

public sealed class OpcionesFiltroOfertasHandler(IOfertaReadRepository repository)
{
    public Task<OpcionesFiltroOfertasDto> HandleAsync(
        OpcionesFiltroOfertasQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return repository.ObtenerOpcionesFiltroAsync(cancellationToken);
    }
}
