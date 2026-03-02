using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ObjectUploadTracking.Commands;

namespace ObjectUploadTracking;

public static class ObjectUploadTrackingRegistration
{
    public static IServiceCollection AddObjectUploadTracking(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.RegisterDbContext<ObjectUploadTrackingDbContext>(configuration);
        services.AddTransient<IConsumeObjectUpload, ConsumeObjectUpload>();
        services.AddTransient<ICreateObjectUpload, CreateObjectUpload>();

        return services;
    }
}
