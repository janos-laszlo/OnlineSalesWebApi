
namespace SalesWebApi.Endpoints;

internal static class CarSalesEndpoints
{
    internal const string CarSalesName = "CarSales";
    internal const string CarSalesBase = "/car-sales";


    internal static void MapCarSalesEndpoints(this WebApplication app)
    {
        var carSalesGroup = app.MapGroup(CarSalesBase)
            .WithTags(CarSalesName);

        carSalesGroup
            .MapGet("", () => Results.Ok("Car sales endpoint is working"));
    }
}
