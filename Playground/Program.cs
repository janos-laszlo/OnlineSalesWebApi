Console.WriteLine($"Processors: {Environment.ProcessorCount}");
//Register<VehicleSalesDbContext>();

static void Register<TDbContext>()
{
    Console.WriteLine($"Registering {typeof(TDbContext).Name}...");
}