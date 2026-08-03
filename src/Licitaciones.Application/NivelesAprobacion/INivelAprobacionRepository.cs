using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.NivelesAprobacion;

public interface INivelAprobacionRepository
{
    Task<IReadOnlyList<NivelAprobacion>> ListarOrdenadosAsync(
        CancellationToken cancellationToken = default);
    Task<NivelAprobacion?> ObtenerAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    Task AgregarAsync(
        NivelAprobacion nivel,
        CancellationToken cancellationToken = default);
    Task GuardarAsync(CancellationToken cancellationToken = default);
    Task EliminarAsync(
        NivelAprobacion nivel,
        CancellationToken cancellationToken = default);
}
