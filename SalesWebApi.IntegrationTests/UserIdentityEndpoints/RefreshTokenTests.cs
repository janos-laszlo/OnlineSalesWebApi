using UserIdentity.Commands;

namespace SalesWebApi.IntegrationTests.UserIdentityEndpoints;

[Collection(UserIdentityFixture.CollectionName)]
public sealed class RefreshTokenTests(UserIdentityFixture fixture)
{
    [Fact]
    public async Task Succeeds_for_valid_refresh_token()
    {
        // Arrange

        // Act & Assert
        var loginResponse = await fixture.RegisterAndLoginUser(
            new UserCredentialsDto(UserUtils.NextEmail, "Password1"));
        var refreshTokenResult = await fixture.Client.PostAsJsonAsync(
            UserIdentityUris.RefreshTokenUri,
            new RefreshTokenRequestDto(loginResponse.RefreshToken),
            TestContext.Current.CancellationToken);
        Assert.True(refreshTokenResult.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Fails_for_invalid_refresh_token()
    {
        var refreshTokenResult = await fixture.Client.PostAsJsonAsync(
            UserIdentityUris.RefreshTokenUri,
            new RefreshTokenRequestDto("some invalid refresh token"),
            TestContext.Current.CancellationToken);
        Assert.False(refreshTokenResult.IsSuccessStatusCode);
    }
}
