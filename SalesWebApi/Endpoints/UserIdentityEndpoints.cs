using System.Security.Claims;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using UserIdentity.Commands;
using UserIdentity.Extensions;

namespace SalesWebApi.Endpoints;

internal static class UserIdentityEndpoints
{
    internal const string IdentityName = "Account";
    internal const string IdentityBase = "/account";
    internal const string Register = "/register";
    internal const string Login = "/login";
    internal const string RefreshToken = "/refresh-token";
    internal const string ConfirmEmail = "/confirm-email";
    internal const string Health = "/health";
    internal const string Profile = "/profile";

    internal static void MapIdentityEndpoints(this WebApplication app)
    {
        var accountGroup = app.MapGroup(IdentityBase)
            .WithTags(IdentityName);

        accountGroup.MapPost(
            Register,
            [RequestSizeLimit(1024)] async (
                UserCredentialsDto userRegistrationDto,
                IRegisterUserCommand registerUserCommand,
                CancellationToken cancellationToken)
            =>
                await registerUserCommand
                    .Execute(userRegistrationDto, cancellationToken)
                    .Finally(result => result.IsSuccess
                        ? Results.Ok()
                        : Results.Problem(
                            title: "User registration failed",
                            detail: result.Error,
                            statusCode: StatusCodes.Status400BadRequest)));

        accountGroup.MapPost(
            Login,
            async (UserCredentialsDto userLoginDto,
                ILoginUserCommand loginUserCommand,
                CancellationToken cancellationToken)
            =>
                await loginUserCommand
                    .Execute(userLoginDto, cancellationToken)
                    .Finally(loginResult => loginResult.IsSuccess
                        ? Results.Ok(loginResult.Value)
                        : Results.Problem(
                            title: "User login failed",
                            detail: loginResult.Error,
                            statusCode: StatusCodes.Status400BadRequest)));

        accountGroup.MapPost(
            RefreshToken,
            async (RefreshTokenRequestDto refreshTokenDto,
                IRefreshTokenCommand refreshTokenCommand,
                CancellationToken cancellationToken)
            =>
                await refreshTokenCommand
                    .Execute(refreshTokenDto.RefreshToken, cancellationToken)
                    .Finally(refreshToken => refreshToken.IsSuccess
                        ? Results.Ok(refreshToken.Value)
                        : Results.Problem(
                            title: "Refreshing token failed",
                            detail: refreshToken.Error,
                            statusCode: StatusCodes.Status400BadRequest)));

        accountGroup.MapGet(
            ConfirmEmail,
            async (string token,
                IConfirmEmailCommand confirmEmailCommand,
                CancellationToken cancellationToken)
            =>
                await confirmEmailCommand
                    .Execute(token, cancellationToken)
                    .Finally(confirmEmailCommandResult => confirmEmailCommandResult.IsSuccess
                        ? Results.Ok()
                        : Results.Problem(
                            title: "Email confirmation failed",
                            detail: confirmEmailCommandResult.Error,
                            statusCode: StatusCodes.Status400BadRequest)));

        accountGroup
            .MapGet(
                Profile,
                async (IGetUserProfileCommand getUserInfoCommand,
                    ClaimsPrincipal httpContextUser,
                    CancellationToken cancellationToken)
                =>
                    await getUserInfoCommand
                        .Execute(httpContextUser.UserId, cancellationToken)
                        .Finally(result => result.IsSuccess
                            ? Results.Ok(result.Value)
                            : Results.Problem(
                                title: "Getting user info failed",
                                detail: result.Error,
                                statusCode: StatusCodes.Status404NotFound)))
            .RequireAuthorization();

        accountGroup
            .MapPut(
                Profile,
                async (UserProfileRequestDto userProfile,
                    IUpdateUserProfileCommand setUserProfileCommand,
                    ClaimsPrincipal httpContextUser,
                    CancellationToken cancellationToken)
                =>
                    await setUserProfileCommand
                        .Execute(httpContextUser.UserId, userProfile, cancellationToken)
                        .Finally(result => result.IsSuccess
                            ? Results.Ok()
                            : Results.Problem(
                                title: "Profile update failed",
                                detail: result.Error,
                                statusCode: StatusCodes.Status400BadRequest)))
            .RequireAuthorization();

        accountGroup.MapGet(Health, () => Results.Ok("Healthy"))
            .RequireAuthorization();
    }
}
