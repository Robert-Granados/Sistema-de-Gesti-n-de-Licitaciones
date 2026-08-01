namespace Licitaciones.Application.Ofertas.Registrar;

public sealed record RegistrarOfertaCommand(
    Guid LicitacionId,
    Guid ProveedorId,
    decimal MontoOfertadoCrc);
