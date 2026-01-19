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
                ? Results.Ok(new { Token = loginResult.Value })
                : Results.BadRequest(Envelope.Failure(loginResult.Error));
        });
        app.MapPost("/refresh-token", () => "Refresh Token Endpoint");
    }
}
