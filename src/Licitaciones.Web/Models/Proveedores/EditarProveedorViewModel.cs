using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.Proveedores;

public sealed class EditarProveedorViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
    [StringLength(
        200,
        ErrorMessage = "El nombre no puede superar los 200 caracteres.")]
    [RegularExpression(
        @"^[\p{L}\p{N}\s.,()]+$",
        ErrorMessage =
            "El nombre solo puede contener letras, números, espacios, punto, coma o paréntesis.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    public int RowVersion { get; set; }
}

