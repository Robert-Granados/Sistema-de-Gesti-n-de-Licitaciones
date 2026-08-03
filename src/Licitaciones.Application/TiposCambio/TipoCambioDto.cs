namespace Licitaciones.Application.TiposCambio;

public sealed record TipoCambioDto(
    Guid Id,
    decimal CrcPorUsd,
    DateTimeOffset FechaVigencia,
    bool Activo);
