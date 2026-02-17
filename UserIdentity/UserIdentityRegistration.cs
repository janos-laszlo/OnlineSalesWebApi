using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        services.AddTransient<IGetUserProfileCommand, GetUserProfileCommand>();
        services.AddTransient<IUpdateUserProfileCommand, UpdateUserProfileCommand>();
        services.RegisterDbContext<UserIdentityDbContext>(configuration);
        services.AddTickerQ();
        services.AddDataProtection();
        services.AddTransient<JwtService>();
        services.AddTransient<EmailConfirmationRequest>();
        services.AddTransient<EmailConfirmationToken>();

        return services;
    }

    public static IHost UseUserIdentity(this IHost app)
    {
        app.UseTickerQ();
        return app;
    }
}
