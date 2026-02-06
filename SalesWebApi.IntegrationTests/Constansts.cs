using SalesWebApi.Endpoints;

namespace SalesWebApi.IntegrationTests;

internal static class Endpoints
{
    internal const string RegisterUri = $"{IdentityEndpoints.IdentityBase}{IdentityEndpoints.Register}";
    internal const string LoginUri = $"{IdentityEndpoints.IdentityBase}{IdentityEndpoints.Login}";
    internal const string RefreshTokenUri = $"{IdentityEndpoints.IdentityBase}{IdentityEndpoints.RefreshToken}";
    internal const string ConfirmEmailUri = $"{IdentityEndpoints.IdentityBase}{IdentityEndpoints.ConfirmEmail}?token=";
    internal const string HealthUri = $"{IdentityEndpoints.IdentityBase}{IdentityEndpoints.Health}";
}
