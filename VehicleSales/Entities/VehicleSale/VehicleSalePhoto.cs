namespace VehicleSales.Entities.VehicleSale;

public enum VehicleSaleStatus { Draft, Active }

public sealed class VehicleSalePhoto
{
    public int Id { get; set; }
    public int VehicleSaleId { get; set; }
    public string ObjectKey { get; set; } = string.Empty; // e.g. "vehicle-sales/{saleId}/{guid}.jpg"
    public int DisplayOrder { get; set; }
}