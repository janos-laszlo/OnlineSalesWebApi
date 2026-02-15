using Common;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using UserIdentity;
using UserIdentity.Commands;
using UserIdentity.Emails;

namespace SalesWebApi.IntegrationTests;

public sealed class UserIdentityFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> app;
    private readonly IServiceScope scope;
    private readonly UserIdentityDbContext userIdentityDbContext;
    internal HttpClient Client { get; }
    internal EmailServiceStub EmailService { get; }
    internal EmailConfirmationToken AnotherEmailConfirmationToken { get; }

    public UserIdentityFixture()
    {
        Constants.ConfigKeys.ConnectionStringKey = "MariaDBIntegrationTests";
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
        AnotherEmailConfirmationToken = CreateAnotherEmailConfirmationToken(app.Services);
        userIdentityDbContext = scope.ServiceProvider.GetRequiredService<UserIdentityDbContext>();
        userIdentityDbContext.Database.EnsureCreated();
    }

    private static EmailConfirmationToken CreateAnotherEmailConfirmationToken(IServiceProvider sp)
    {
        var dataProtectionProvider = sp.GetRequiredService<IDataProtectionProvider>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();
        configuration[EmailConfirmationToken.PurposeKey] =
            "some different key than the one used in the app, to ensure that tokens created by this instance are not valid in the app";
        return new EmailConfirmationToken(dataProtectionProvider, configuration);
    }

    internal async Task<TokenResponseDto> RegisterAndLoginUser(UserCredentialsDto credentials)
    {
        var registrationResult = await this.Client.PostAsJsonAsync(
            Endpoints.RegisterUri, credentials);
        Assert.True(registrationResult.IsSuccessStatusCode);

        var loginResult = await this.Client.PostAsJsonAsync(
            Endpoints.LoginUri, credentials);
        Assert.True(loginResult.IsSuccessStatusCode);

        var body = await loginResult.Content.ReadFromJsonAsync<TokenResponseDto>();
        Assert.NotNull(body);
        return body;
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
{ }
