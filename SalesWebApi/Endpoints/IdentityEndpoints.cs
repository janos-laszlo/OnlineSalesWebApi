using Microsoft.AspNetCore.Mvc;
using UserIdentity.Commands;

namespace SalesWebApi.Endpoints;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this WebApplication app)
    {
        app.MapPost(
            "/register",
            [RequestSizeLimit(1024)] async (UserCredentialsDto userRegistrationDto,
            IRegisterUserCommand registerUserCommand,
            CancellationToken cancellationToken) =>
            {
                var registerUserCommandResult = await registerUserCommand.Execute(
                    userRegistrationDto, cancellationToken);
                return registerUserCommandResult.IsSuccess
                    ? Results.Ok()
                    : Results.Problem(
                        title: "User registration failed",
                        detail: registerUserCommandResult.Error,
                        statusCode: 400);
            });

        app.MapPost(
            "/login",
            async (UserCredentialsDto userLoginDto,
            ILoginUserCommand loginUserCommand,
            CancellationToken cancellationToken) =>
        {
            var loginResult = await loginUserCommand.Execute(userLoginDto, cancellationToken);
            return loginResult.IsSuccess
                ? Results.Ok(loginResult.Value)
                    : Results.Problem(
                        title: "User login failed",
                        detail: loginResult.Error,
                        statusCode: 400);
        });

        app.MapPost(
            "/refresh-token",
            async (RefreshTokenRequestDto refreshTokenDto,
            IRefreshTokenCommand refreshTokenCommand,
            CancellationToken cancellationToken) =>
            {
                var refreshToken = await refreshTokenCommand.Execute(
                    refreshTokenDto.RefreshToken, cancellationToken);
                return refreshToken.IsSuccess
                    ? Results.Ok(refreshToken.Value)
                    : Results.Problem(
                        title: "Refreshing token failed",
                        detail: refreshToken.Error,
                        statusCode: 400);
            });

        app.MapGet(
            "/confirm-email",
            async (string token,
            IConfirmEmailCommand confirmEmailCommand,
            CancellationToken cancellationToken) =>
            {
                var confirmEmailCommandResult = await confirmEmailCommand.Execute(token, cancellationToken);
                return confirmEmailCommandResult.IsSuccess
                    ? Results.Ok()
                    : Results.Problem(
                        title: "Email confirmation failed",
                        detail: confirmEmailCommandResult.Error,
                        statusCode: 400);
            });

        app.MapGet("/health", () => Results.Ok("Healthy"))
            .RequireAuthorization();
    }
}
