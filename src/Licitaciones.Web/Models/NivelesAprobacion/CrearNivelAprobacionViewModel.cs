using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.NivelesAprobacion;

public sealed class CrearNivelAprobacionViewModel : IValidatableObject
{
    [Required(ErrorMessage = "El monto mínimo es obligatorio.")]
    [Range(
        0,
        double.MaxValue,
        ErrorMessage = "El monto mínimo no puede ser negativo.")]
    [Display(Name = "Monto mínimo (₡)")]
    public decimal MontoMinimoCrc { get; set; }

    [Range(
        0,
        double.MaxValue,
        ErrorMessage = "El monto máximo no puede ser negativo.")]
    [Display(Name = "Monto máximo (₡)")]
    public decimal? MontoMaximoCrc { get; set; }

    [Required(ErrorMessage = "El aprobador es obligatorio.")]
    [StringLength(
        200,
        ErrorMessage = "El aprobador no puede superar los 200 caracteres.")]
    [Display(Name = "Aprobador")]
    public string Aprobador { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MontoMaximoCrc.HasValue && MontoMaximoCrc.Value <= MontoMinimoCrc)
        {
            yield return new ValidationResult(
                "El monto máximo debe ser mayor que el monto mínimo.",
                [nameof(MontoMaximoCrc)]);
        }
    }
}
