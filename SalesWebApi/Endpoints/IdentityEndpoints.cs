public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this WebApplication app)
    {
        app.MapPost("/register", () => "Register Endpoint");
        app.MapPost("/login", () => "Login Endpoint");
        app.MapPost("/refresh-token", () => "Refresh Token Endpoint");
    }
}
