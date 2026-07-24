namespace Licitaciones.Domain.Entities;

public sealed class TipoCambio
{
    private TipoCambio()
    {
    }

    public TipoCambio(
        decimal crcPorUsd,
        DateTimeOffset fechaVigencia,
        bool activo = false)
    {
        if (crcPorUsd <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(crcPorUsd),
                "El tipo de cambio debe ser mayor que cero.");
        }

        if (fechaVigencia == default)
        {
            throw new ArgumentException(
                "La fecha de vigencia es obligatoria.",
                nameof(fechaVigencia));
        }

        Id = Guid.NewGuid();
        CrcPorUsd = crcPorUsd;
        FechaVigencia = fechaVigencia;
        Activo = activo;
    }

    public Guid Id { get; private set; }

    public decimal CrcPorUsd { get; private set; }

    public DateTimeOffset FechaVigencia { get; private set; }

    public bool Activo { get; private set; }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;
}
