using Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using ObjectUploadTracking;
using UserIdentity;
using VehicleSales;

namespace SalesWebApi.IntegrationTests;

public sealed class DatabaseMigrationFixture : IAsyncLifetime
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private WebApplicationFactory<Program> app;
    private UserIdentityDbContext userIdentityDbContext;
    private VehicleSalesDbContext vehicleSalesDbContext;
    private ObjectUploadTrackingDbContext objectUploadTrackingDbContext;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public async ValueTask InitializeAsync()
    {
        Constants.ConfigKeys.ConnectionStringKey = "MariaDBIntegrationTests";
        R2Config.SectionKey = "R2Testing";

        app = new WebApplicationFactory<Program>();
        var scope = app.Services.CreateAsyncScope();

        userIdentityDbContext = scope.ServiceProvider.GetRequiredService<UserIdentityDbContext>();
        vehicleSalesDbContext = scope.ServiceProvider.GetRequiredService<VehicleSalesDbContext>();
        objectUploadTrackingDbContext = scope.ServiceProvider.GetRequiredService<ObjectUploadTrackingDbContext>();
        await userIdentityDbContext.Database.MigrateAsync();
        await vehicleSalesDbContext.Database.MigrateAsync();
        await objectUploadTrackingDbContext.Database.MigrateAsync();
        await ClearTables();
    }

    public async ValueTask DisposeAsync()
    {
        await ClearTables();

        await app.DisposeAsync();
    }

    private async Task ClearTables() => 
        await Task.WhenAll(
            this.objectUploadTrackingDbContext.ObjectUploads.ExecuteDeleteAsync(),
            this.vehicleSalesDbContext.VehicleSales.ExecuteDeleteAsync(),
            this.userIdentityDbContext.Users.ExecuteDeleteAsync());
}