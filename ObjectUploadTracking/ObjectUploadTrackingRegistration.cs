using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ObjectUploadTracking;

// TODO: Add background service to clean up expired object uploads
public static class ObjectUploadTrackingRegistration
{
    public static IServiceCollection AddObjectUploadTracking(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.RegisterDbContext<ObjectUploadTrackingDbContext>(configuration);
        services.AddScoped<IObjectUploadOperations, ObjectUploadOperations>();

        return services;
    }
}
