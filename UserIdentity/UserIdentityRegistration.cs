using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TickerQ.DependencyInjection;
using UserIdentity.Commands;
using UserIdentity.Emails;

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
        services.AddTransient<IConfirmEmailCommand, ConfirmEmailCommand>();
        var connectionString = configuration.GetConnectionString("MariaDB");
        services.AddDbContext<UserIdentityDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)));
        services.AddTickerQ();
        services.AddDataProtection();
        services.AddTransient<JwtService>();
        services.AddSingleton<IEmailService, ConsoleEmailService>();

        return services;
    }

    public static void UseUserIdentity(this IApplicationBuilder app)
    {
        app.UseTickerQ();
    }
}
