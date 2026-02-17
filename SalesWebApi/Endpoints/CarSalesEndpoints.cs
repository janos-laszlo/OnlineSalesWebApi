using CarSales.Queries;

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
            .MapGet(
                "makes",
                async (IGetCarMakesQuery query,
                CancellationToken cancellationToken) =>
                    Results.Ok(await query.Get(cancellationToken)));

        carSalesGroup
            .MapGet(
                "models",
                async (string makeName,
                IGetMakeModelsQuery query,
                CancellationToken cancellationToken) =>
                    Results.Ok(await query.Get(makeName, cancellationToken)));
    }
}
