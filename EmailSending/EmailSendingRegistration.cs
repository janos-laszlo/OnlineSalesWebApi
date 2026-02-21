using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailSending;

public static class EmailSendingRegistration
{
    public static IServiceCollection AddEmailSending(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.RegisterDbContext<EmailSendingDbContext>(configuration);
        services.AddScoped<IResilientEmailService, ConsoleEmailService>();
        services.AddScoped<INonResilientEmailService, ConsoleEmailService>();
        return services;
    }
}
