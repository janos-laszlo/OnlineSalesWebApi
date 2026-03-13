using System.Security.Claims;
using CSharpFunctionalExtensions;
using UserIdentity.Extensions;
using VehicleSales.Commands;
using VehicleSales.Dtos;
using VehicleSales.Queries;

namespace SalesWebApi.Endpoints;

internal static class VehicleSalesEndpoints
{
    internal const string VehicleSalesName = "VehicleSales";
    internal const string VehicleSalesBase = "/vehicle-sales";
    internal const string ConfirmObjectUpload = "/confirm-object-upload/{objectUploadId:int}";
    internal const string Makes = "/makes";
    internal const string Models = "/models";
    internal const string ById = "/{id:int}";

    internal static void MapVehicleSalesEndpoints(this WebApplication app)
    {
        var vehicleSalesGroup = app.MapGroup(VehicleSalesBase)
            .WithTags(VehicleSalesName);

        vehicleSalesGroup
            .MapGet(
                Makes,
                async (IGetVehicleMakesQuery query,
                CancellationToken cancellationToken) =>
                    Results.Ok(await query.Get(cancellationToken)));

        vehicleSalesGroup
            .MapGet(
                Models,
                async (string makeName,
                IGetMakeModelsQuery query,
                CancellationToken cancellationToken) =>
                    Results.Ok(await query.Get(makeName, cancellationToken)));

        vehicleSalesGroup
            .MapPost(
                string.Empty,
                async (CreateVehicleSaleRequestDto dto,
                    ICreateVehicleSale create,
                    ClaimsPrincipal principal,
                    CancellationToken cancellationToken)
                =>
                    await create.Execute(dto, principal.UserId, cancellationToken)
                        .Finally(result => result.IsSuccess
                            ? Results.Created($"{VehicleSalesBase}/{result.Value.EntityId}", result.Value)
                            : Results.Problem(
                                title: "Vehicle sale creation failed",
                                detail: result.Error,
                                statusCode: StatusCodes.Status400BadRequest)))
            .RequireAuthorization();

        vehicleSalesGroup
            .MapPatch(
                ConfirmObjectUpload,
                async (int objectUploadId,
                    IConfirmObjectUploadForVehicleSale confirm,
                    ClaimsPrincipal principal,
                    CancellationToken cancellationToken)
                =>
                    await confirm.Execute(objectUploadId, principal.UserId, cancellationToken)
                        .Finally(result => result.IsSuccess
                            ? Results.NoContent()
                            : Results.Problem(
                                title: "Object upload confirmation failed",
                                detail: result.Error,
                                statusCode: StatusCodes.Status400BadRequest)))
            .RequireAuthorization();

        vehicleSalesGroup
            .MapGet(
                ById,
                async (int id,
                IGetVehicleSale query,
                CancellationToken cancellationToken) =>
                {
                     var result = await query.Execute(id, cancellationToken);
                     return result is not null
                         ? Results.Ok(result)
                         : Results.NotFound();
                });

        vehicleSalesGroup
            .MapGet(
                string.Empty,
                async ([AsParameters] PagedRequest request,
                IGetVehicleSales query,
                CancellationToken cancellationToken) =>
                    Results.Ok(await query.Execute(request, cancellationToken)));

        vehicleSalesGroup
            .MapPatch(
                "{vehicleSaleId:int}",
                async (int vehicleSaleId,
                    UpdateVehicleSaleRequestDto dto,
                    IUpdateVehicleSale update,
                    ClaimsPrincipal principal,
                    CancellationToken cancellationToken) 
                =>
                    // TODO: Return proper status codes for different failure cases
                    // (e.g. 404 if sale not found, 403 if user not authorized to update this sale, etc.)
                    // Since there will be translation as well, return error codes from the 
                    // application layer and map them to appropriate HTTP status codes here.
                    // Document the possible error codes in the application layer and their meanings,
                    // so it's clear what each code represents.
                    await update.Execute(vehicleSaleId, principal.UserId, dto, cancellationToken)
                        .Finally(result => result.IsSuccess
                            ? Results.Ok(result.Value)
                            : Results.Problem(
                                title: "Vehicle sale update failed",
                                detail: result.Error,
                                statusCode: StatusCodes.Status400BadRequest)))
            .RequireAuthorization();
    }
}