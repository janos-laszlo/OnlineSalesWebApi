using Microsoft.AspNetCore.Mvc;
using UserIdentity.Commands;

namespace SalesWebApi.Endpoints;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this WebApplication app)
    {
        app.MapPost(
            "/register", 
            [RequestSizeLimit(1024)](UserRegistrationDto userRegistrationDto, 
            RegisterUserCommand registerUserCommand) =>
            {
                var registerUserCommandResult = registerUserCommand.Execute(userRegistrationDto);
                return registerUserCommandResult.IsSuccess
                    ? Results.Ok()
                    : Results.BadRequest(Envelope.Failure(registerUserCommandResult.Error));
            });
        app.MapPost("/login", () => "Login Endpoint");
        app.MapPost("/refresh-token", () => "Refresh Token Endpoint");
    }
}
