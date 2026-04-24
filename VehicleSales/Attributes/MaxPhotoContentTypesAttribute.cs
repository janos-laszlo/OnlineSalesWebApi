using Microsoft.AspNetCore.Http;
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
        var photos = (IFormFileCollection?)value;
        if (photos is null || photos.Count == 0)
            return ValidationResult.Success;

        if (photos.Count > maxPhotos)
            return new ValidationResult($"Max {maxPhotos} photos allowed.");

        if (photos.Any(f => !AllowedImageContentTypes.Contains(f.ContentType)))
            return new ValidationResult("Invalid image content type.");

        return ValidationResult.Success;
    }
}

