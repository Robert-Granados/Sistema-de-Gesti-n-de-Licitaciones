using Licitaciones.Domain.Enums;

namespace Licitaciones.Domain.Entities;

public sealed class Licitacion
{
    private Licitacion()
    {
        Codigo = null!;
        Titulo = null!;
    }

    public Licitacion(
        string codigo,
        string titulo,
        DateTimeOffset fechaCierre,
        decimal presupuestoEstimadoCrc)
    {
        Codigo = ValidarTextoObligatorio(codigo, nameof(codigo));
        Titulo = ValidarTextoObligatorio(titulo, nameof(titulo));

        if (fechaCierre == default)
        {
            throw new ArgumentException("La fecha de cierre es obligatoria.", nameof(fechaCierre));
        }

        if (presupuestoEstimadoCrc <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(presupuestoEstimadoCrc),
                "El presupuesto estimado debe ser mayor que cero.");
        }

        Id = Guid.NewGuid();
        FechaCierre = fechaCierre;
        PresupuestoEstimadoCrc = presupuestoEstimadoCrc;
        Estado = EstadoLicitacion.Borrador;
    }

    public Guid Id { get; private set; }

    public string Codigo { get; private set; }

    public string Titulo { get; private set; }

    public EstadoLicitacion Estado { get; private set; }

    public DateTimeOffset FechaCierre { get; private set; }

    public decimal PresupuestoEstimadoCrc { get; private set; }

    public void Publicar(DateTimeOffset ahora)
    {
        if (Estado != EstadoLicitacion.Borrador)
        {
            throw new InvalidOperationException(
                "Solo una licitación en borrador puede publicarse.");
        }

        if (FechaCierre <= ahora)
        {
            throw new InvalidOperationException(
                "La fecha de cierre debe ser futura para publicar la licitación.");
        }

        Estado = EstadoLicitacion.Publicada;
    }

    public void Cerrar()
    {
        if (Estado == EstadoLicitacion.Cerrada)
        {
            throw new InvalidOperationException("La licitación ya está cerrada.");
        }

        Estado = EstadoLicitacion.Cerrada;
    }

    private static string ValidarTextoObligatorio(string? valor, string nombreParametro)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("El valor es obligatorio.", nombreParametro);
        }

        return valor.Trim();
    }
}

