using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserIdentity.Commands;

namespace UserIdentity;

public static class UserIdentityRegistration
{
    public static IServiceCollection AddUserIdentity(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddTransient<IRegisterUserCommand, RegisterUserCommand>();
        services.AddDbContext<UserIdentityDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("MariaDB");
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString));
        });
        return services;
    }
}