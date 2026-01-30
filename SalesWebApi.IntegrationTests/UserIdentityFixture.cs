using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using UserIdentity;
using UserIdentity.Emails;

namespace SalesWebApi.IntegrationTests;

public sealed class UserIdentityFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> app;
    private readonly IServiceScope scope;
    private readonly UserIdentityDbContext userIdentityDbContext;
    internal HttpClient Client { get; }
    internal EmailServiceStub EmailService { get; }    

    public UserIdentityFixture()
    {
        UserIdentityDbContext.ConnectionStringKey = "MariaDBIntegrationTests";
        app = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IEmailService, EmailServiceStub>();
                });
            });
        Client = app.CreateClient();
        scope = app.Services.CreateScope();
        EmailService = (EmailServiceStub)app.Services.GetRequiredService<IEmailService>();
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

[CollectionDefinition("User Identity")]
public class UserIdentityCollection : ICollectionFixture<UserIdentityFixture>
{}
