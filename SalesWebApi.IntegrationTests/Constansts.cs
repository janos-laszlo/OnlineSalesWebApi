using SalesWebApi.Endpoints;

namespace SalesWebApi.IntegrationTests;

internal static class Endpoints
{
    internal const string RegisterUri = $"{UserIdentityEndpoints.IdentityBase}{UserIdentityEndpoints.Register}";
    internal const string LoginUri = $"{UserIdentityEndpoints.IdentityBase}{UserIdentityEndpoints.Login}";
    internal const string RefreshTokenUri = $"{UserIdentityEndpoints.IdentityBase}{UserIdentityEndpoints.RefreshToken}";
    internal const string ConfirmEmailUri = $"{UserIdentityEndpoints.IdentityBase}{UserIdentityEndpoints.ConfirmEmail}?token=";
    internal const string HealthUri = $"{UserIdentityEndpoints.IdentityBase}{UserIdentityEndpoints.Health}";
    internal const string Profile = $"{UserIdentityEndpoints.IdentityBase}{UserIdentityEndpoints.Profile}";
}
