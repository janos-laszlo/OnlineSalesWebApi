using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace VehicleSales.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class MaxPhotoContentTypesAttribute(int maxPhotos) : ValidationAttribute
{
    public static readonly HashSet<string> AllowedImageContentTypes =
    [
        Image.Jpeg,
        Image.Png,
        Image.Webp,
        Image.Bmp,
        Image.Tiff,
        Image.Avif,
    ];

    protected override ValidationResult? IsValid(
        object? value, ValidationContext validationContext)
    {
        var photoContentTypes = (IReadOnlyList<string>?)value;
        if (photoContentTypes is null)
            return ValidationResult.Success;

        if (photoContentTypes.Any(string.IsNullOrWhiteSpace))
            return new ValidationResult("Null or empty photo content type.");

        if (photoContentTypes.Count > maxPhotos)
            return new ValidationResult($"Max {maxPhotos} photos allowed.");

        if (photoContentTypes.Any(p => !AllowedImageContentTypes.Contains(p)))
            return new ValidationResult("Invalid image content type.");

        return ValidationResult.Success;
    }
}
