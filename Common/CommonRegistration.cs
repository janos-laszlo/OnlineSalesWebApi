using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Common;

public static class Constants
{
    public static string ConnectionStringKey { get; set; } = "MariaDB";

    public static class Tables
    {
        public const string Users = "Users";
    }
}

public static class CommonRegistration
{
    public static void RegisterDbContext<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration) where TDbContext : DbContext
    {
        var connectionString = configuration.GetConnectionString(Constants.ConnectionStringKey);
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException(
                $"Connection string '{Constants.ConnectionStringKey}' is not configured.");
        services.AddDbContext<TDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                efOptions => efOptions.SchemaBehavior(MySqlSchemaBehavior.Throw)));
    }
}
