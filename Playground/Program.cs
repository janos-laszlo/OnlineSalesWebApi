using CarSales;

Console.WriteLine($"Processors: {Environment.ProcessorCount}");
//Register<CarSalesDbContext>();

static void Register<TDbContext>()
{
    Console.WriteLine($"Registering {typeof(TDbContext).Name}...");
}