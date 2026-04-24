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

        // Queries
        services.AddTransient<IGetVehicleMakesQuery, GetVehicleMakesQuery>();
        services.AddTransient<IGetMakeModelsQuery, GetMakeModelsQuery>();
        services.AddTransient<IGetVehicleSales, GetVehicleSales>();
        services.AddTransient<IGetVehicleSale, GetVehicleSale>();
        
        // Commands
        services.AddTransient<ICreateVehicleSale, CreateVehicleSale>();
        services.AddTransient<IUpdateVehicleSale, UpdateVehicleSale>();
        services.AddTransient<IDeleteVehicleSale, DeleteVehicleSale>();

        RegisterCloudflareR2(services, configuration);

        return services;
    }

    private static void RegisterCloudflareR2(IServiceCollection services, IConfiguration configuration)
    {
        var r2Config = configuration.GetSection(R2Config.SectionKey).Get<R2Config>() ??
            throw new InvalidOperationException("Cloudflare R2 configuration section is missing or invalid.");
        if (string.IsNullOrEmpty(r2Config.AccountId) ||
            string.IsNullOrEmpty(r2Config.AccessKeyId) ||
            string.IsNullOrEmpty(r2Config.SecretAccessKey))
        {
            throw new InvalidOperationException("Cloudflare R2 credentials are not properly configured.");
        }

        var credentials = new BasicAWSCredentials(r2Config.AccessKeyId, r2Config.SecretAccessKey);
        services.AddScoped<IAmazonS3>(
            sp => new AmazonS3Client(credentials, new AmazonS3Config
            {
                ServiceURL = $"https://{r2Config.AccountId}.r2.cloudflarestorage.com"
            }));
    }
}
