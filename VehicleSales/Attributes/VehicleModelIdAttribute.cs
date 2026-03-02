using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace VehicleSales.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class VehicleModelIdAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(
        object? value, ValidationContext validationContext)
    {
        var dbContext = validationContext.GetRequiredService<VehicleSalesDbContext>();

        var vehicleModelId = (int?)value;
        if (vehicleModelId is null)
            return ValidationResult.Success;

        var exists = dbContext.VehicleModels.Any(vehicleModel => vehicleModel.Id == vehicleModelId);
        return exists
            ? ValidationResult.Success
            : new ValidationResult("Vehicle model doesn't exist.");
    }
}
