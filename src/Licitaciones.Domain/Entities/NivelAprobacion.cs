namespace Licitaciones.Domain.Entities;

public sealed class NivelAprobacion
{
    private NivelAprobacion()
    {
        Aprobador = null!;
    }

    public NivelAprobacion(
        decimal montoMinimoCrc,
        decimal? montoMaximoCrc,
        string aprobador)
    {
        if (montoMinimoCrc < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(montoMinimoCrc),
                "El monto mínimo no puede ser negativo.");
        }

        if (montoMaximoCrc.HasValue && montoMaximoCrc.Value <= montoMinimoCrc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(montoMaximoCrc),
                "El monto máximo debe ser mayor que el monto mínimo.");
        }

        if (string.IsNullOrWhiteSpace(aprobador))
        {
            throw new ArgumentException("El aprobador es obligatorio.", nameof(aprobador));
        }

        Id = Guid.NewGuid();
        MontoMinimoCrc = montoMinimoCrc;
        MontoMaximoCrc = montoMaximoCrc;
        Aprobador = aprobador.Trim();
    }

    public Guid Id { get; private set; }

    public decimal MontoMinimoCrc { get; private set; }

    public decimal? MontoMaximoCrc { get; private set; }

    public string Aprobador { get; private set; }

    public bool Contiene(decimal monto) =>
        monto >= MontoMinimoCrc
        && (!MontoMaximoCrc.HasValue || monto <= MontoMaximoCrc.Value);
}

