namespace Licitaciones.Application.Common.Models;

public interface IPaginaResultado
{
    int TotalRegistros { get; }

    int PaginaActual { get; }

    int TotalPaginas { get; }

    bool TienePaginaAnterior { get; }

    bool TienePaginaSiguiente { get; }
}

public sealed class PaginaResultado<T> : IPaginaResultado
{
    public PaginaResultado(
        IReadOnlyList<T> elementos,
        int totalRegistros,
        int paginaActual,
        int tamanoPagina)
    {
        ArgumentNullException.ThrowIfNull(elementos);

        if (totalRegistros < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalRegistros));
        }

        if (paginaActual < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(paginaActual));
        }

        if (tamanoPagina < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(tamanoPagina));
        }

        Elementos = elementos;
        TotalRegistros = totalRegistros;
        PaginaActual = paginaActual;
        TamanoPagina = tamanoPagina;
        TotalPaginas = totalRegistros == 0
            ? 0
            : (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
    }

    public IReadOnlyList<T> Elementos { get; }

    public int TotalRegistros { get; }

    public int PaginaActual { get; }

    public int TamanoPagina { get; }

    public int TotalPaginas { get; }

    public bool TienePaginaAnterior => PaginaActual > 1;

    public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
}

