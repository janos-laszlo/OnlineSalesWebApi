using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ObjectUploadTracking;
using UserIdentity;
using VehicleSales;

[assembly: AssemblyFixture(typeof(SalesWebApi.IntegrationTests.DatabaseMigrationFixture))]

namespace SalesWebApi.IntegrationTests;

public sealed class DatabaseMigrationFixture : IAsyncLifetime
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private WebApplicationFactory<Program> app;
    private IAmazonS3 r2Client;
    private UserIdentityDbContext userIdentityDbContext;
    private VehicleSalesDbContext vehicleSalesDbContext;
    private ObjectUploadTrackingDbContext objectUploadTrackingDbContext;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public async ValueTask InitializeAsync()
    {
        Common.Constants.ConfigKeys.ConnectionStringKey = "MariaDBIntegrationTests";
        R2Config.SectionKey = "R2Testing";

        app = new WebApplicationFactory<Program>();
        var scope = app.Services.CreateAsyncScope();

        r2Client = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        userIdentityDbContext = scope.ServiceProvider.GetRequiredService<UserIdentityDbContext>();
        vehicleSalesDbContext = scope.ServiceProvider.GetRequiredService<VehicleSalesDbContext>();
        objectUploadTrackingDbContext = scope.ServiceProvider.GetRequiredService<ObjectUploadTrackingDbContext>();
        await userIdentityDbContext.Database.MigrateAsync();
        await vehicleSalesDbContext.Database.MigrateAsync();
        await objectUploadTrackingDbContext.Database.MigrateAsync();
        await ClearTestData();
    }

    public async ValueTask DisposeAsync()
    {
        await app.DisposeAsync();
    }

    private async Task ClearTestData()
    {
        await ClearTables();
        await ClearR2Bucket();
    }

    private async Task ClearTables() =>
        await Task.WhenAll(
            this.objectUploadTrackingDbContext.ObjectUploads.ExecuteDeleteAsync(),
            this.vehicleSalesDbContext.VehicleSales.ExecuteDeleteAsync(),
            this.userIdentityDbContext.Users.ExecuteDeleteAsync());

    private async Task ClearR2Bucket()
    {
        var configuration = app.Services.GetRequiredService<IConfiguration>();
        var bucketName = configuration[R2Config.BucketNameKey];

        string? continuationToken = null;

        do
        {
            var listRequest = new ListObjectsV2Request
            {
                BucketName = bucketName,
                ContinuationToken = continuationToken
            };

            var listResponse = await r2Client.ListObjectsV2Async(listRequest);

            if (listResponse.S3Objects?.Count > 0 == true)
            {
                var deleteRequest = new DeleteObjectsRequest
                {
                    BucketName = bucketName,
                    Objects = [.. listResponse.S3Objects.Select(o => new KeyVersion { Key = o.Key })]
                };

                await r2Client.DeleteObjectsAsync(deleteRequest);
            }

            continuationToken = listResponse.NextContinuationToken;

            if (listResponse.IsTruncated != true)
                break;
        }
        while (true);
    }
}
