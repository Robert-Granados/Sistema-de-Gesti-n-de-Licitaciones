using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Licitaciones.Web.Validation;

[AttributeUsage(AttributeTargets.Property)]
public sealed class FechaFuturaAttribute : ValidationAttribute, IClientModelValidator
{
    public FechaFuturaAttribute()
        : base("La fecha indicada debe ser futura.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not DateTimeOffset fecha)
        {
            return ValidationResult.Success;
        }

        return fecha <= DateTimeOffset.UtcNow
            ? new ValidationResult(ErrorMessage)
            : ValidationResult.Success;
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        context.Attributes["data-val"] = "true";
        context.Attributes["data-val-fechafutura"] =
            ErrorMessage ?? "La fecha indicada debe ser futura.";
    }
}
