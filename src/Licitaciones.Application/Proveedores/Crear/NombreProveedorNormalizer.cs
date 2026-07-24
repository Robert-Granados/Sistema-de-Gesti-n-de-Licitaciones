using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Licitaciones.Application.Proveedores.Crear;

public static partial class NombreProveedorNormalizer
{
    public static string Limpiar(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return string.Empty;
        }

        return EspaciosRepetidosRegex().Replace(nombre.Trim(), " ");
    }

    public static bool EsValido(string nombre) =>
        nombre.Length is > 0 and <= 200
        && CaracteresPermitidosRegex().IsMatch(nombre);

    public static string Normalizar(string nombre)
    {
        var limpio = Limpiar(nombre).Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder(limpio.Length);

        foreach (var caracter in limpio)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter)
                != UnicodeCategory.NonSpacingMark)
            {
                resultado.Append(caracter);
            }
        }

        return resultado
            .ToString()
            .ToUpperInvariant()
            .Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex(@"^[\p{L}\p{N}\s.,()]+$", RegexOptions.CultureInvariant)]
    private static partial Regex CaracteresPermitidosRegex();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex EspaciosRepetidosRegex();
}

