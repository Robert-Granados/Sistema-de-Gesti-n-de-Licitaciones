using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.Ofertas.Ports;

public interface IOfertaWriteRepository
{
    Task AgregarAsync(
        Oferta oferta,
        CancellationToken cancellationToken = default);

    Task<Oferta?> ObtenerPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task ActualizarAsync(
        Oferta oferta,
        CancellationToken cancellationToken = default);

    Task EliminarAsync(
        Oferta oferta,
        CancellationToken cancellationToken = default);
}
