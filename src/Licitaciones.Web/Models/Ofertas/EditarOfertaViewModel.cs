using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.Ofertas;

public sealed class EditarOfertaViewModel
{
    public Guid Id { get; set; }

    public Guid LicitacionId { get; set; }

    public string CodigoLicitacion { get; set; } = string.Empty;

    public string NombreProveedor { get; set; } = string.Empty;

    [Required(ErrorMessage = "El monto ofertado es obligatorio.")]
    [Range(
        0.01,
        double.MaxValue,
        ErrorMessage = "El monto ofertado debe ser mayor que cero.")]
    [Display(Name = "Monto ofertado (CRC)")]
    public decimal MontoOfertadoCrc { get; set; }
}
