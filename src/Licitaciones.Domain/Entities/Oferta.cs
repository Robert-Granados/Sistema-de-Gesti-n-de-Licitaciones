namespace Licitaciones.Domain.Entities;

public sealed class Oferta
{
    private Oferta()
    {
        Licitacion = null!;
        Proveedor = null!;
    }

    public Oferta(
        Guid licitacionId,
        Guid proveedorId,
        decimal montoOfertadoCrc,
        DateTimeOffset fechaRegistro)
    {
        if (licitacionId == Guid.Empty)
        {
            throw new ArgumentException("La licitación es obligatoria.", nameof(licitacionId));
        }

        if (proveedorId == Guid.Empty)
        {
            throw new ArgumentException("El proveedor es obligatorio.", nameof(proveedorId));
        }

        if (montoOfertadoCrc <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(montoOfertadoCrc),
                "El monto ofertado debe ser mayor que cero.");
        }

        if (fechaRegistro == default)
        {
            throw new ArgumentException("La fecha de registro es obligatoria.", nameof(fechaRegistro));
        }

        Id = Guid.NewGuid();
        LicitacionId = licitacionId;
        ProveedorId = proveedorId;
        MontoOfertadoCrc = montoOfertadoCrc;
        FechaRegistro = fechaRegistro;
        Licitacion = null!;
        Proveedor = null!;
    }

    public Guid Id { get; private set; }

    public Guid LicitacionId { get; private set; }

    public Guid ProveedorId { get; private set; }

    public decimal MontoOfertadoCrc { get; private set; }

    public DateTimeOffset FechaRegistro { get; private set; }

    public Licitacion Licitacion { get; private set; }

    public Proveedor Proveedor { get; private set; }
}
