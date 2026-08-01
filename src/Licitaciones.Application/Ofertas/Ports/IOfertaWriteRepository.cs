namespace Licitaciones.Application.Ofertas.Ports;

public interface IOfertaWriteRepository
{
    Task AgregarAsync(
        Domain.Entities.Oferta oferta,
        CancellationToken cancellationToken = default);
}
