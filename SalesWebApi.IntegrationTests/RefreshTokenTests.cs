using UserIdentity.Commands;

namespace SalesWebApi.IntegrationTests;

[Collection("User Identity")]
public sealed class RefreshTokenTests(UserIdentityFixture fixture)
{
    [Fact]
    public async Task Succeeds_for_valid_refresh_token()
    {
        // Arrange

        // Act & Assert
        var loginResponse = await RegisterAndLoginUser();
        var refreshTokenResult = await fixture.Client.PostAsJsonAsync(
            "refresh-token", new RefreshTokenRequestDto(loginResponse.RefreshToken));
        Assert.True(refreshTokenResult.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Fails_for_invalid_refresh_token()
    {
        var refreshTokenResult = await fixture.Client.PostAsJsonAsync(
            "refresh-token", new RefreshTokenRequestDto("some invalid refresh token"));
        Assert.False(refreshTokenResult.IsSuccessStatusCode);
    }

    private async Task<TokenResponseDto> RegisterAndLoginUser()
    {
        UserCredentialsDto credentials = new(UserUtils.NextEmail, "Password1");
        var registrationResult = await fixture.Client.PostAsJsonAsync(
            "register", credentials);
        Assert.True(registrationResult.IsSuccessStatusCode);

        var loginResult = await fixture.Client.PostAsJsonAsync(
            "login", credentials);
        Assert.True(loginResult.IsSuccessStatusCode);

        var body = await loginResult.Content.ReadFromJsonAsync<TokenResponseDto>();
        Assert.NotNull(body);
        return body;
    }
}
