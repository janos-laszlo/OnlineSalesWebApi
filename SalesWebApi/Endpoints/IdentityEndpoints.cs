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
                    ? Results.Ok(new { Id = registerUserCommandResult.Value })
                    : Results.BadRequest(Envelope.Failure(registerUserCommandResult.Error));
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
                : Results.BadRequest(Envelope.Failure(loginResult.Error));
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
                    : Results.BadRequest(Envelope.Failure(refreshToken.Error));
            });

        app.MapGet("/health", () => Results.Ok("Healthy"))
            .RequireAuthorization();
    }
}
