using Common;
using DraftEntities.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DraftEntities;

public static class DraftEntitiesRegistration
{
    public static void AddDraftEntities(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.RegisterDbContext<DraftEntitiesDbContext>(configuration);
        services.AddScoped<IDraftEntityOperations, DraftEntityOperations>();

        // Register your draft entities here
        // Example:
        // services.AddScoped<IDraftEntityService, DraftEntityService>();
    }
}
