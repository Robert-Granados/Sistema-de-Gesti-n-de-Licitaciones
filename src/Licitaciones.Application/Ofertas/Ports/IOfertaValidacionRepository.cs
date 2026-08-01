namespace Licitaciones.Application.Ofertas.Ports;

public interface IOfertaValidacionRepository
{
    Task<bool> ExisteLicitacionPublicadaAsync(
        Guid licitacionId,
        CancellationToken cancellationToken = default);

    Task<DateTimeOffset?> ObtenerFechaCierreAsync(
        Guid licitacionId,
        CancellationToken cancellationToken = default);

    Task<decimal> ObtenerPresupuestoAsync(
        Guid licitacionId,
        CancellationToken cancellationToken = default);

    Task<bool> ProveedorExisteAsync(
        Guid proveedorId,
        CancellationToken cancellationToken = default);

    Task<bool> YaTieneOfertaAsync(
        Guid licitacionId,
        Guid proveedorId,
        CancellationToken cancellationToken = default);
}
