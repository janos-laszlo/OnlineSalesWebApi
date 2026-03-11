using System.Net.Http.Json;
using EmailSending;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserIdentity.Commands;
using UserIdentity.Emails;

namespace SalesWebApi.IntegrationTests;

public sealed class UserIdentityFixture : IDisposable
{
    internal const string CollectionName = "User Identity";
    private readonly WebApplicationFactory<Program> app;
    internal HttpClient Client { get; }
    internal EmailServiceStub EmailService { get; }
    internal EmailConfirmationToken AnotherEmailConfirmationToken { get; }

    public UserIdentityFixture()
    {
        app = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IResilientEmailService, EmailServiceStub>();
                });
            });
        Client = app.CreateClient();
        EmailService = (EmailServiceStub)app.Services.GetRequiredService<IResilientEmailService>();
        AnotherEmailConfirmationToken = CreateAnotherEmailConfirmationToken(app.Services);
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
            UserIdentityUris.RegisterUri, credentials);
        Assert.True(registrationResult.IsSuccessStatusCode);

        var loginResult = await this.Client.PostAsJsonAsync(
            UserIdentityUris.LoginUri, credentials);
        Assert.True(loginResult.IsSuccessStatusCode);

        var body = await loginResult.Content.ReadFromJsonAsync<TokenResponseDto>();
        Assert.NotNull(body);
        return body;
    }

    public void Dispose()
    {
        Client.Dispose();
        app.Dispose();
    }
}

[CollectionDefinition(UserIdentityFixture.CollectionName)]
public class UserIdentityCollection : ICollectionFixture<UserIdentityFixture>
{ }
