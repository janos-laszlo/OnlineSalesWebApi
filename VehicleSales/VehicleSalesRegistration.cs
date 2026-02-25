using Amazon.Runtime;
using Amazon.S3;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VehicleSales.Commands;
using VehicleSales.Queries;

namespace VehicleSales;

public static class VehicleSalesRegistration
{
    public static IServiceCollection AddVehicleSales(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.RegisterDbContext<VehicleSalesDbContext>(configuration);

        services.AddTransient<IGetVehicleMakesQuery, GetVehicleMakesQuery>();
        services.AddTransient<IGetMakeModelsQuery, GetMakeModelsQuery>();
        services.AddTransient<ICreateVehicleSale, CreateVehicleSale>();
        services.AddTransient<IGetVehicleSales, GetVehicleSales>();

        RegisterCloudflareR2(services, configuration);

        return services;
    }

    private static void RegisterCloudflareR2(IServiceCollection services, IConfiguration configuration)
    {
        var accountId = configuration["R2:AccountId"];
        var accessKey = configuration["R2:AccessKeyId"];
        var secretKey = configuration["R2:SecretAccessKey"];
        if (string.IsNullOrEmpty(accessKey) ||
            string.IsNullOrEmpty(secretKey) ||
            string.IsNullOrEmpty(accountId))
        {
            throw new InvalidOperationException("Cloudflare R2 credentials are not properly configured.");
        }

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        services.AddScoped<IAmazonS3>(
            sp => new AmazonS3Client(credentials, new AmazonS3Config
            {
                ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com"
            }));
    }
}
