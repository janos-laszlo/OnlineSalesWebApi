namespace VehicleSales.Entities.Translation;

internal sealed class Translation
{
    public int Id { get; private set; }
    public required string Key { get; init; }
    public required string Value { get; set; }
}
