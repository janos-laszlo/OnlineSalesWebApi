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
                            ? Results.Created($"/vehicle-sales/{result.Value.SaleId}", result.Value)
                            : Results.Problem(detail: result.Error, statusCode: 400)))
            .RequireAuthorization();

        vehicleSalesGroup
            .MapPost(
                "{saleId:int}/photos/confirm",
                async (int saleId,
                    IConfirmVehicleSalePhotos confirm,
                    ClaimsPrincipal principal,
                    CancellationToken cancellationToken) =>
                    await confirm.Execute(saleId, principal.UserId, cancellationToken)
                        .Finally(result => result.IsSuccess
                            ? Results.NoContent()
                            : Results.Problem(detail: result.Error, statusCode: 400)))
            .RequireAuthorization();

        vehicleSalesGroup
            .MapPost(
                "{saleId:int}/photos/refresh-upload-urls",
                async (int saleId,
                    IRefreshPhotoUploadUrls refresh,
                    ClaimsPrincipal principal,
                    CancellationToken cancellationToken) =>
                    await refresh.Execute(saleId, principal.UserId, cancellationToken)
                        .Finally(result => result.IsSuccess
                            ? Results.Ok(result.Value)
                            : Results.Problem(detail: result.Error, statusCode: 400)))
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
