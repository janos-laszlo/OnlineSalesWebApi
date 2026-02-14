using UserIdentity.Commands;

namespace SalesWebApi.IntegrationTests.Commands;

[Collection("User Identity")]
public sealed class GetUserProfileTests(UserIdentityFixture fixture)
{
    [Fact]
    public async Task Succeeds_for_authenticated_user()
    {
        // Arrange
        var tokenResponse = await fixture.RegisterAndLoginUser(
            new UserCredentialsDto(UserUtils.NextEmail, "Password1"));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, Endpoints.Profile);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", tokenResponse.AccessToken);
        var result = await fixture.Client.SendAsync(request);

        // Assert
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Fails_for_unauthenticated_user()
    {
        // Arrange

        // Act
        var result = await fixture.Client.GetAsync(Endpoints.Profile);

        // Assert
        Assert.False(result.IsSuccessStatusCode);
    }
}
