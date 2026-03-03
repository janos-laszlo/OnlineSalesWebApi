using Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using ObjectUploadTracking;
using UserIdentity;
using UserIdentity.Commands;
using VehicleSales;

namespace SalesWebApi.IntegrationTests;

public sealed class VehicleSalesFixture : IDisposable
{
    internal const string CollectionName = "Vehicle Sales";
    private readonly WebApplicationFactory<Program> app;
    private readonly IServiceScope scope;
    private readonly UserIdentityDbContext userIdentityDbContext;
    private readonly VehicleSalesDbContext vehicleSalesDbContext;
    private readonly ObjectUploadTrackingDbContext objectUploadTrackingDbContext;
    internal HttpClient Client { get; }
    internal string AccessToken { get; }

    public VehicleSalesFixture()
    {
        Constants.ConfigKeys.ConnectionStringKey = "MariaDBIntegrationTests";
        R2Config.SectionKey = "R2Testing";
        app = new WebApplicationFactory<Program>();
        Client = app.CreateClient();
        scope = app.Services.CreateScope();
        userIdentityDbContext = scope.ServiceProvider.GetRequiredService<UserIdentityDbContext>();
        userIdentityDbContext.Database.Migrate();
        vehicleSalesDbContext = scope.ServiceProvider.GetRequiredService<VehicleSalesDbContext>();
        vehicleSalesDbContext.Database.Migrate();
        objectUploadTrackingDbContext = scope.ServiceProvider.GetRequiredService<ObjectUploadTrackingDbContext>();
        objectUploadTrackingDbContext.Database.Migrate();
        AccessToken = RegisterAndLoginUser(new UserCredentialsDto(UserUtils.NextEmail, "Password1")).AccessToken;
    }

    private TokenResponseDto RegisterAndLoginUser(UserCredentialsDto credentials)
    {
        var registrationResult = this.Client
            .PostAsJsonAsync(UserIdentityUris.RegisterUri, credentials)
            .GetAwaiter()
            .GetResult();
        Assert.True(registrationResult.IsSuccessStatusCode);

        var loginResult = this.Client
            .PostAsJsonAsync(UserIdentityUris.LoginUri, credentials)
            .GetAwaiter()
            .GetResult();
        Assert.True(loginResult.IsSuccessStatusCode);

        var body = loginResult.Content.ReadFromJsonAsync<TokenResponseDto>()
            .GetAwaiter()
            .GetResult();
        Assert.NotNull(body);
        return body;
    }

    public void Dispose()
    {
        this.objectUploadTrackingDbContext.ObjectUploads.ExecuteDelete();
        this.vehicleSalesDbContext.VehicleSales.ExecuteDelete();
        this.userIdentityDbContext.Users.ExecuteDelete();
        scope.Dispose();
        Client.Dispose();
        app.Dispose();
    }
}

[CollectionDefinition(VehicleSalesFixture.CollectionName)]
public class VehicleSalesCollection : ICollectionFixture<VehicleSalesFixture>
{ }
