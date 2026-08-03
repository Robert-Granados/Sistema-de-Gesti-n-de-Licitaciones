using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.TiposCambio;

public sealed class EditarTipoCambioViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El tipo de cambio es obligatorio.")]
    [Range(
        0.01,
        double.MaxValue,
        ErrorMessage = "El tipo de cambio debe ser mayor que cero.")]
    [Display(Name = "Tipo de cambio (₡ por USD)")]
    public decimal CrcPorUsd { get; set; }

    [Required(ErrorMessage = "La fecha de vigencia es obligatoria.")]
    [Display(Name = "Fecha de vigencia")]
    public DateTimeOffset? FechaVigencia { get; set; }

    [Display(Name = "Tipo de cambio activo")]
    public bool Activo { get; set; }
}
