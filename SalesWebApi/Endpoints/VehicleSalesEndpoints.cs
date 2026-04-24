using System.Security.Claims;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserIdentity.Extensions;
using VehicleSales.Commands;
using VehicleSales.Commands.Translations;
using VehicleSales.Dtos;
using VehicleSales.Queries;

namespace SalesWebApi.Endpoints;

internal static class VehicleSalesEndpoints
{
    internal const string VehicleSalesName = "VehicleSales";
    internal const string VehicleSalesBase = "/vehicle-sales";
    internal const string TranslationsBase = "/translations";
    internal const string Makes = "/makes";
    internal const string Models = "/models";
    internal const string My = "/my";
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
                    Results.Ok(await query.Get(cancellationToken)))
            .WithSummary("Get all vehicle makes");

        vehicleSalesGroup
            .MapGet(
                Models,
                async (string makeName,
                IGetMakeModelsQuery query,
                CancellationToken cancellationToken) =>
                    Results.Ok(await query.Get(makeName, cancellationToken)))
            .WithSummary("Get models for a specific vehicle make");

        vehicleSalesGroup
            .MapPost(
                string.Empty,
                async ([FromForm] CreateVehicleSaleRequestDto dto,
                    IFormFileCollection photos,
                    ICreateVehicleSale create,
                    ClaimsPrincipal principal,
                    CancellationToken cancellationToken)
                =>
                    await create.Execute(dto with { Photos = photos }, principal.UserId, cancellationToken)
                        .Finally(result => result.IsSuccess
                            ? Results.Created($"{VehicleSalesBase}/{result.Value}", null)
                            : Results.Problem(
                                title: "Vehicle sale creation failed",
                                detail: result.Error,
                                statusCode: StatusCodes.Status400BadRequest)))
            .RequireAuthorization()
            .DisableAntiforgery()
            .WithSummary("Create a new vehicle sale");

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
                })
            .WithSummary("Get a vehicle sale by ID");

        vehicleSalesGroup
            .MapGet(
                string.Empty,
                async ([AsParameters] PagedRequest request,
                IGetVehicleSales query,
                CancellationToken cancellationToken) =>
                    Results.Ok(await query.Execute(request, cancellationToken)))
            .WithSummary("Get a paged list of vehicle sales");

        vehicleSalesGroup
            .MapGet(
                My,
                async ([AsParameters] PagedRequest request,
                IGetUserVehicleSales query,
                ClaimsPrincipal principal,
                CancellationToken cancellationToken) =>
                    Results.Ok(await query.Execute(principal.UserId, request, cancellationToken)))
            .RequireAuthorization()
            .WithSummary("Get vehicle sales of the authenticated user");

        vehicleSalesGroup
            .MapPatch(
                "{vehicleSaleId:int}",
                async (int vehicleSaleId,
                    [FromForm] UpdateVehicleSaleRequestDto dto,
                    IFormFileCollection photos,
                    IUpdateVehicleSale update,
                    ClaimsPrincipal principal,
                    CancellationToken cancellationToken)
                =>
                    await update.Execute(vehicleSaleId, principal.UserId, dto, photos, cancellationToken)
                        .Finally(result => result.IsSuccess
                            ? Results.NoContent()
                            : Results.Problem(
                                title: "Vehicle sale update failed",
                                detail: result.Error.ToString(),
                                statusCode: StatusCodes.Status400BadRequest)))
            .RequireAuthorization()
            .DisableAntiforgery()
            .WithSummary("Update a vehicle sale");

        vehicleSalesGroup
            .MapDelete(
                "{vehicleSaleId:int}",
                async (int vehicleSaleId,
                    IDeleteVehicleSale delete,
                    ClaimsPrincipal principal,
                    CancellationToken cancellationToken)
                =>
                    await delete.Execute(vehicleSaleId, principal.UserId, cancellationToken)
                        .Finally(result => result.IsSuccess
                            ? Results.NoContent()
                            : Results.Problem(
                                title: "Vehicle sale deletion failed",
                                detail: result.Error.ToString(),
                                statusCode: result.Error switch
                                {
                                    DeleteVehicleSaleErrorCode.VehicleSaleNotFound => StatusCodes.Status404NotFound,
                                    DeleteVehicleSaleErrorCode.UnauthorizedToDelete => StatusCodes.Status403Forbidden,
                                    _ => StatusCodes.Status400BadRequest
                                })))
            .RequireAuthorization()
            .WithSummary("Delete a vehicle sale");

        var translationsGroup = app.MapGroup(TranslationsBase)
            .WithTags("Translations");

        translationsGroup
            .MapPost(
                string.Empty,
                async ([FromBody] CreateTranslationDto dto,
                    ICreateTranslation command,
                    CancellationToken cancellationToken)
                =>
                    await command.Execute(dto, cancellationToken)
                        .Finally(result => result.IsSuccess
                            ? Results.Created($"{TranslationsBase}/{result.Value}", null)
                            : Results.Problem(
                                title: "Translation creation failed",
                                detail: result.Error,
                                statusCode: StatusCodes.Status400BadRequest)))
            .RequireAuthorization("Admin")
            .WithSummary("Create a new translation");
    }
}