namespace VehicleSales.Entities.VehicleMake;

internal sealed class VehicleMake
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public ICollection<VehicleModel> VehicleModels { get; private set; } = [];

    private VehicleMake(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
