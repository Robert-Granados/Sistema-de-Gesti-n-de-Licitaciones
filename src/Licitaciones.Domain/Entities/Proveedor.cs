using System.Text.RegularExpressions;

namespace Licitaciones.Domain.Entities;

public sealed partial class Proveedor
{
    private readonly List<Oferta> _ofertas = [];

    private Proveedor()
    {
        Nombre = null!;
        NombreNormalizado = null!;
    }

    public Proveedor(string nombre)
        : this(nombre, nombre.Trim().ToUpperInvariant())
    {
    }

    public Proveedor(string nombre, string nombreNormalizado)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre del proveedor es obligatorio.", nameof(nombre));
        }

        if (!CaracteresPermitidosRegex().IsMatch(nombre))
        {
            throw new ArgumentException(
                "El nombre contiene caracteres no permitidos.",
                nameof(nombre));
        }

        if (string.IsNullOrWhiteSpace(nombreNormalizado))
        {
            throw new ArgumentException(
                "El nombre normalizado es obligatorio.",
                nameof(nombreNormalizado));
        }

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        NombreNormalizado = nombreNormalizado;
    }

    public Guid Id { get; private set; }

    public string Nombre { get; private set; }

    public string NombreNormalizado { get; private set; }

    public DateTimeOffset? EliminadoEn { get; private set; }

    public bool EstaEliminado => EliminadoEn.HasValue;

    public IReadOnlyCollection<Oferta> Ofertas => _ofertas;

    public void CambiarNombre(string nombre, string nombreNormalizado)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre del proveedor es obligatorio.",
                nameof(nombre));
        }

        if (!CaracteresPermitidosRegex().IsMatch(nombre))
        {
            throw new ArgumentException(
                "El nombre contiene caracteres no permitidos.",
                nameof(nombre));
        }

        if (string.IsNullOrWhiteSpace(nombreNormalizado))
        {
            throw new ArgumentException(
                "El nombre normalizado es obligatorio.",
                nameof(nombreNormalizado));
        }

        Nombre = nombre.Trim();
        NombreNormalizado = nombreNormalizado;
    }

    public void Eliminar(DateTimeOffset eliminadoEn)
    {
        if (EstaEliminado)
        {
            throw new InvalidOperationException("El proveedor ya fue eliminado.");
        }

        EliminadoEn = eliminadoEn;
    }

    [GeneratedRegex(@"^[\p{L}\p{N}\s.,()]+$", RegexOptions.CultureInvariant)]
    private static partial Regex CaracteresPermitidosRegex();
}
