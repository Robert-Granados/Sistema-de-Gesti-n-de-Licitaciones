using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.TiposCambio;

public sealed class CrearTipoCambioViewModel
{
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

    [Display(Name = "Activar este tipo de cambio")]
    public bool Activar { get; set; }
}
