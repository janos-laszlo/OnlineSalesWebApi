using UserIdentityUri = SalesWebApi.Endpoints.UserIdentityEndpoints;
using VehicleSalesUri = SalesWebApi.Endpoints.VehicleSalesEndpoints;

namespace SalesWebApi.IntegrationTests;

internal static class UserIdentityUris
{
    internal const string RegisterUri = $"{UserIdentityUri.IdentityBase}{UserIdentityUri.Register}";
    internal const string LoginUri = $"{UserIdentityUri.IdentityBase}{UserIdentityUri.Login}";
    internal const string RefreshTokenUri = $"{UserIdentityUri.IdentityBase}{UserIdentityUri.RefreshToken}";
    internal const string ConfirmEmailUri = $"{UserIdentityUri.IdentityBase}/confirm-email/";
    internal const string HealthUri = $"{UserIdentityUri.IdentityBase}{UserIdentityUri.Health}";
    internal const string ProfileUri = $"{UserIdentityUri.IdentityBase}{UserIdentityUri.Profile}";
}

internal static class VehicleSalesUris
{
    internal const string VehicleMakesUri = $"{VehicleSalesUri.VehicleSalesBase}{VehicleSalesUri.Makes}";
    internal const string VehicleModelsUri = $"{VehicleSalesUri.VehicleSalesBase}{VehicleSalesUri.Models}";
    internal const string ConfirmObjectUpload = $"{VehicleSalesUri.VehicleSalesBase}/confirm-object-upload/";
}
