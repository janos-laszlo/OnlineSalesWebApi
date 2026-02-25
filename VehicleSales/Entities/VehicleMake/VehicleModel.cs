namespace VehicleSales.Entities.VehicleMake;

internal sealed class VehicleModel
{
    public int Id { get; private set; }
    public string Name { get; private set; }

    public int VehicleMakeId { get; private set; }
    public VehicleMake VehicleMake { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private VehicleModel(int id, string name, int vehicleMakeId)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        Id = id;
        Name = name;
        VehicleMakeId = vehicleMakeId;
    }
}
