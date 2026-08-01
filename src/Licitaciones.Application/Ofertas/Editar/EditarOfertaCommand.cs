namespace Licitaciones.Application.Ofertas.Editar;

public sealed record EditarOfertaCommand(
    Guid Id,
    decimal MontoOfertadoCrc);
