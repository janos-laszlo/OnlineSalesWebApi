using Microsoft.AspNetCore.Mvc;
using UserIdentity.Commands;

namespace SalesWebApi.Endpoints;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this WebApplication app)
    {
        app.MapPost(
            "/register",
            [RequestSizeLimit(1024)] (UserCredentialsDto userRegistrationDto,
            IRegisterUserCommand registerUserCommand) =>
            {
                var registerUserCommandResult = registerUserCommand.Execute(userRegistrationDto);
                return registerUserCommandResult.IsSuccess
                    ? Results.Ok(new { Id = registerUserCommandResult.Value })
                    : Results.BadRequest(Envelope.Failure(registerUserCommandResult.Error));
            });
        app.MapPost(
            "/login",
            (UserCredentialsDto userLoginDto,
            ILoginUserCommand loginUserCommand) =>
        {
            var loginResult = loginUserCommand.Execute(userLoginDto);
            return loginResult.IsSuccess
                ? Results.Ok(loginResult.Value)
                : Results.BadRequest(Envelope.Failure(loginResult.Error));
        });
        app.MapPost(
            "/refresh-token",
            (RefreshTokenRequestDto refreshTokenDto,
            IRefreshTokenCommand refreshTokenCommand) =>
            {
                var refreshToken = refreshTokenCommand.Execute(refreshTokenDto.RefreshToken);
                return refreshToken.IsSuccess
                    ? Results.Ok(refreshToken.Value)
                    : Results.BadRequest(Envelope.Failure(refreshToken.Error));
            });

        app.MapGet("/health", () => Results.Ok("Healthy"))
            .RequireAuthorization();
    }
}
