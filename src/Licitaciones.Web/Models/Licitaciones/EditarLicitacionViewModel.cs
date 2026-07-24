using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.Licitaciones;

public sealed class EditarLicitacionViewModel
{
    public Guid Id { get; set; }

    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El título de la licitación es obligatorio.")]
    [StringLength(
        300,
        ErrorMessage = "El título no puede superar los 300 caracteres.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de cierre es obligatoria.")]
    [Display(Name = "Fecha y hora de cierre")]
    public DateTimeOffset? FechaCierre { get; set; }

    [Required(ErrorMessage = "El presupuesto estimado es obligatorio.")]
    [Range(
        0.01,
        double.MaxValue,
        ErrorMessage = "El presupuesto estimado debe ser mayor que cero.")]
    [Display(Name = "Presupuesto estimado (CRC)")]
    public decimal PresupuestoEstimadoCrc { get; set; }

    public int RowVersion { get; set; }
}
