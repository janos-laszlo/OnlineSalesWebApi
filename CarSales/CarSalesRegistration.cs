using CarSales.Queries;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarSales;

public static class CarSalesRegistration
{
    public static IServiceCollection AddCarSales(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.RegisterDbContext<CarSalesDbContext>(configuration);

        services.AddScoped<IGetUserPostsQuery, GetUserPostsQuery>();
        
        return services;
    }
}
