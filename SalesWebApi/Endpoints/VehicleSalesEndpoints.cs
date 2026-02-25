using System.Security.Claims;
using CSharpFunctionalExtensions;
using UserIdentity.Extensions;
using VehicleSales.Commands;
using VehicleSales.Queries;

namespace SalesWebApi.Endpoints;

internal static class VehicleSalesEndpoints
{
    internal const string VehicleSalesName = "VehicleSales";
    internal const string VehicleSalesBase = "/vehicle-sales";


    internal static void MapVehicleSalesEndpoints(this WebApplication app)
    {
        var vehicleSalesGroup = app.MapGroup(VehicleSalesBase)
            .WithTags(VehicleSalesName);

        vehicleSalesGroup
            .MapGet(
                "makes",
                async (IGetVehicleMakesQuery query,
                CancellationToken cancellationToken) =>
                    Results.Ok(await query.Get(cancellationToken)));

        vehicleSalesGroup
            .MapGet(
                "models",
                async (string makeName,
                IGetMakeModelsQuery query,
                CancellationToken cancellationToken) =>
                    Results.Ok(await query.Get(makeName, cancellationToken)));

        vehicleSalesGroup
            .MapPost(
                string.Empty,
                async (CreateVehicleSaleDto dto,
                    ICreateVehicleSale create,
                    ClaimsPrincipal principal,
                    CancellationToken cancellationToken)
                =>
                    await create.Execute(dto, principal.UserId, cancellationToken)
                        .Finally(result => result.IsSuccess
                            ? Results.Ok()
                            : Results.Problem(
                                title: "Vehicle sale creation failed",
                                detail: result.Error,
                                statusCode: StatusCodes.Status400BadRequest)))
            .RequireAuthorization();

        vehicleSalesGroup
            .MapGet(
                string.Empty,
                async ([AsParameters] PagedRequest request,
                IGetVehicleSales query,
                CancellationToken cancellationToken) =>
                    Results.Ok(await query.Execute(request, cancellationToken)));
    }
}
