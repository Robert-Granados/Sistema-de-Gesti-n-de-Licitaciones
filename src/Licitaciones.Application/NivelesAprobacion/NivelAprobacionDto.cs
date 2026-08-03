namespace Licitaciones.Application.NivelesAprobacion;

public sealed record NivelAprobacionDto(
    Guid Id,
    decimal MontoMinimoCrc,
    decimal? MontoMaximoCrc,
    string Aprobador);
