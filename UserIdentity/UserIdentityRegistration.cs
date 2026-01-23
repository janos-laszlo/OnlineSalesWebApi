using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TickerQ.DependencyInjection;
using UserIdentity.Commands;

namespace UserIdentity;

public static class UserIdentityRegistration
{
    public static IServiceCollection AddUserIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IRegisterUserCommand, RegisterUserCommand>();
        services.AddTransient<ILoginUserCommand, LoginUserCommand>();
        services.AddTransient<IRefreshTokenCommand, RefreshTokenCommand>();
        services.AddTickerQ();
        services.AddDbContext<UserIdentityDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("MariaDB");
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString));
        });
        services.AddDataProtection();
        services.AddTransient<JwtService>();

        return services;
    }

    public static void UseUserIdentity(this IApplicationBuilder app)
    {
        app.UseTickerQ();
    }
}