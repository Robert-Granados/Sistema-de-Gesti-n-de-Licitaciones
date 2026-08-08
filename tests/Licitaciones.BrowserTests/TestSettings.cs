using System.Globalization;

namespace Licitaciones.BrowserTests;

/// <summary>
/// Configuración compartida de las pruebas funcionales (HU-46).
/// La URL base apunta a la aplicación levantada vía Docker Compose;
/// se puede sobreescribir con la variable de entorno APP_BASE_URL.
/// </summary>
public static class TestSettings
{
    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("APP_BASE_URL") ?? "http://localhost:8080";

    public static string SufijoUnico() => Guid.NewGuid().ToString("N")[..12];

    public static string NombreProveedor(string sufijo) => $"Proveedor E2E {sufijo}";

    public static string CodigoLicitacion(string sufijo) =>
        $"LIC-E2E-{sufijo}".ToUpperInvariant();

    public static string FechaCierreFutura() =>
        DateTime.UtcNow.AddDays(20).ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);

    public static string Monto(decimal valor) =>
        valor.ToString("0.00", CultureInfo.InvariantCulture);
}
