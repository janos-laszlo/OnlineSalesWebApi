namespace VehicleSales.Storage;

public interface IStorageService
{
    /// <summary>Generates a presigned URL for a direct PUT upload.</summary>
    Task<Uri> GenerateUploadUrlAsync(string objectKey, TimeSpan expiry);

    /// <summary>Checks if an object actually exists (used during confirmation).</summary>
    Task<bool> ObjectExistsAsync(string objectKey);

    Task DeleteObjectAsync(string objectKey);
}