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

    public void Actualizar(decimal crcPorUsd, DateTimeOffset fechaVigencia)
    {
        Validar(crcPorUsd, fechaVigencia);
        CrcPorUsd = crcPorUsd;
        FechaVigencia = fechaVigencia;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    private static void Validar(decimal crcPorUsd, DateTimeOffset fechaVigencia)
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
    }
}
