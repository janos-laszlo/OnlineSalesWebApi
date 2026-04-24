namespace VehicleSales;

internal static class Constants
{
    internal const string ModuleName = "VehicleSales";
}

internal sealed record R2Config(
    string AccountId,
    string AccessKeyId,
    string SecretAccessKey,
    string BucketName)
{
    internal static string SectionKey { get; set; } = "R2";
    public static string BucketNameKey => $"{SectionKey}:{nameof(BucketName)}";
}

internal static class Tables
{
    internal const string VehicleSales = "vehicle_sales";
    internal const string VehicleMakes = "vehicle_makes";
    internal const string VehicleModels = "vehicle_models";
    internal const string Translations = "translations";
}
