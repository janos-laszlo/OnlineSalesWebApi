using Microsoft.AspNetCore.Mvc;
using UserIdentity.Commands;

namespace SalesWebApi.Endpoints;

public static class IdentityEndpoints
{
    public const string IdentityName = "Account";
    public const string IdentityBase = "/account";
    public const string Register = "/register";
    public const string Login = "/login";
    public const string RefreshToken = "/refresh-token";
    public const string ConfirmEmail = "/confirm-email";
    public const string Health = "/health";

    public static void MapIdentityEndpoints(this WebApplication app)
    {
        var accountGroup = app.MapGroup(IdentityBase)
            .WithTags(IdentityName);

        accountGroup.MapPost(
            Register,
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

        accountGroup.MapPost(
            Login,
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

        accountGroup.MapPost(
            RefreshToken,
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

        accountGroup.MapGet(
            ConfirmEmail,
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

        accountGroup.MapGet(Health, () => Results.Ok("Healthy"))
            .RequireAuthorization();
    }
}
