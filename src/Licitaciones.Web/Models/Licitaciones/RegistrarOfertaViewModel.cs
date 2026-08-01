using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.Licitaciones;

public sealed class RegistrarOfertaViewModel
{
    public Guid LicitacionId { get; set; }

    [Required(ErrorMessage = "El proveedor es obligatorio.")]
    [Display(Name = "Proveedor")]
    public Guid ProveedorId { get; set; }

    [Required(ErrorMessage = "El monto ofertado es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
    [Display(Name = "Monto ofertado (CRC)")]
    public decimal MontoOfertadoCrc { get; set; }
}
