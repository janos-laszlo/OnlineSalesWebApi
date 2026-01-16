using Microsoft.Extensions.DependencyInjection;
using UserIdentity.Commands;

namespace UserIdentity;

public static class UserIdentityRegistration
{
    public static IServiceCollection AddUserIdentity(this IServiceCollection services)
    {
        services.AddTransient<RegisterUserCommand>();
        return services;
    }
}