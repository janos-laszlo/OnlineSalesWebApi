namespace CarSales.Entities.CarMake;

internal sealed class CarModel
{
    public int Id { get; private set; }
    public string Name { get; private set; }

    public int CarMakeId { get; private set; }
    public CarMake CarMake { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private CarModel(int id, string name, int carMakeId)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        Id = id;
        Name = name;
        CarMakeId = carMakeId;
    }
}
