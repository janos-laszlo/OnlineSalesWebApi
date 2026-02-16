namespace CarSales.Entities.CarMake;

internal sealed class CarMake
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public ICollection<CarModel> CarModels { get; private set; } = [];

    private CarMake(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
