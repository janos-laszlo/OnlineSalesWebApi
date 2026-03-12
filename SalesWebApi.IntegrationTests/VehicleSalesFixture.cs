using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using UserIdentity.Commands;
using VehicleSales.Queries;

namespace SalesWebApi.IntegrationTests;

public sealed class VehicleSalesFixture : IDisposable
{
    internal const string CollectionName = "Vehicle Sales";
    private readonly WebApplicationFactory<Program> app;
    internal HttpClient Client { get; }
    internal HttpClient ExternalClient { get; }
    internal string AccessToken { get; }

    public VehicleSalesFixture()
    {
        app = new WebApplicationFactory<Program>();
        Client = app.CreateClient();
        ExternalClient = new HttpClient();
        AccessToken = RegisterAndLoginUser(new UserCredentialsDto(UserUtils.NextEmail, "Password1")).AccessToken;
    }

    private TokenResponseDto RegisterAndLoginUser(UserCredentialsDto credentials)
    {
        var registrationResult = this.Client
            .PostAsJsonAsync(UserIdentityUris.RegisterUri, credentials)
            .GetAwaiter()
            .GetResult();
        Assert.True(registrationResult.IsSuccessStatusCode);

        var loginResult = this.Client
            .PostAsJsonAsync(UserIdentityUris.LoginUri, credentials)
            .GetAwaiter()
            .GetResult();
        Assert.True(loginResult.IsSuccessStatusCode);

        var body = loginResult.Content.ReadFromJsonAsync<TokenResponseDto>()
            .GetAwaiter()
            .GetResult();
        Assert.NotNull(body);
        return body;
    }

    public void Dispose()
    {
        Client.Dispose();
        ExternalClient.Dispose();
        app.Dispose();
    }

    internal Task<VehicleSaleDto?> GetVehicleSaleAsync(int id) =>
        Client.GetFromJsonAsync<VehicleSaleDto>($"{VehicleSalesUris.GetVehicleSaleById}{id}");

    internal async Task<int> CreateVehicleSaleAsync(string requestBody)
    {
        var httpRequest = CreateVehicleSaleRequest(requestBody);
        var response = await Client.SendAsync(httpRequest, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location?.OriginalString;
        Assert.NotNull(location);
        var idString = location.Split('/').Last();
        return int.Parse(idString);
    }

    internal HttpRequestMessage CreateVehicleSaleRequest(string requestBody)
    {
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post, Endpoints.VehicleSalesEndpoints.VehicleSalesBase)
        {
            Content = new StringContent(requestBody, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", AccessToken);
        return httpRequest;
    }
}

[CollectionDefinition(VehicleSalesFixture.CollectionName)]
public class VehicleSalesCollection : ICollectionFixture<VehicleSalesFixture>
{ }
