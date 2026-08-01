using Licitaciones.Domain.Enums;
using Licitaciones.Domain.Exceptions;

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

    public DateTimeOffset? PublicadaEn { get; private set; }

    public DateTimeOffset? CerradaEn { get; private set; }

    public string? MotivoCierre { get; private set; }

    public DateTimeOffset? EliminadoEn { get; private set; }

    public bool EstaEliminada => EliminadoEn.HasValue;

    public void Publicar(DateTimeOffset ahora)
    {
        if (Estado != EstadoLicitacion.Borrador)
        {
            throw new TransicionEstadoInvalidaException(
                $"No se puede publicar una licitación en estado {Estado}. Solo se permite desde Borrador.");
        }

        if (FechaCierre <= ahora)
        {
            throw new TransicionEstadoInvalidaException(
                "La fecha de cierre debe ser futura para publicar la licitación.");
        }

        Estado = EstadoLicitacion.Publicada;
        PublicadaEn = ahora;
    }

    public void Cerrar(string motivo, DateTimeOffset ahora)
    {
        if (Estado != EstadoLicitacion.Borrador && Estado != EstadoLicitacion.Publicada)
        {
            throw new TransicionEstadoInvalidaException(
                $"No se puede cerrar una licitación en estado {Estado}.");
        }

        MotivoCierre = ValidarTextoObligatorio(motivo, nameof(motivo));
        Estado = EstadoLicitacion.Cerrada;
        CerradaEn = ahora;
    }

    public void CambiarTitulo(string titulo)
    {
        if (Estado != EstadoLicitacion.Borrador && Estado != EstadoLicitacion.Publicada)
        {
            throw new TransicionEstadoInvalidaException(
                $"No se puede editar una licitación en estado {Estado}.");
        }

        Titulo = ValidarTextoObligatorio(titulo, nameof(titulo));
    }

    public void ActualizarFechaCierre(DateTimeOffset fechaCierre)
    {
        if (fechaCierre == default)
        {
            throw new ArgumentException("La fecha de cierre es obligatoria.", nameof(fechaCierre));
        }

        FechaCierre = fechaCierre;
    }

    public void ActualizarPresupuesto(decimal presupuestoEstimadoCrc)
    {
        if (presupuestoEstimadoCrc <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(presupuestoEstimadoCrc),
                "El presupuesto estimado debe ser mayor que cero.");
        }

        PresupuestoEstimadoCrc = presupuestoEstimadoCrc;
    }

    private static string ValidarTextoObligatorio(string? valor, string nombreParametro)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("El valor es obligatorio.", nombreParametro);
        }

        return valor.Trim();
    }

    public void Eliminar(DateTimeOffset eliminadoEn)
    {
        if (EstaEliminada)
        {
            throw new InvalidOperationException("La licitación ya fue eliminada.");
        }

        EliminadoEn = eliminadoEn;
    }
}

