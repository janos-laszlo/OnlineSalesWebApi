using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using UserIdentity;

namespace SalesWebApi.IntegrationTests;

public sealed class UserIdentityFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> app;
    private readonly IServiceScope scope;
    private readonly UserIdentityDbContext userIdentityDbContext;
    public HttpClient Client { get; }

    public UserIdentityFixture()
    {
        UserIdentityDbContext.ConnectionStringKey = "MariaDBIntegrationTests";
        app = new WebApplicationFactory<Program>();
        Client = app.CreateClient();
        scope = app.Services.CreateScope();
        userIdentityDbContext = scope.ServiceProvider.GetRequiredService<UserIdentityDbContext>();
        userIdentityDbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        this.userIdentityDbContext.Users.ExecuteDelete();
        scope.Dispose();
        Client.Dispose();
        app.Dispose();
    }
}
