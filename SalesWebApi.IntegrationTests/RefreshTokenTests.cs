using UserIdentity.Commands;

namespace SalesWebApi.IntegrationTests;

public sealed class RefreshTokenTests : IClassFixture<UserIdentityFixture>
{
    private readonly UserIdentityFixture fixture;

    public RefreshTokenTests(UserIdentityFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task Succeeds_for_valid_refresh_token()
    {
        // Arrange

        // Act & Assert
        var loginResponse = await RegisterAndLoginUser();
        var refreshTokenResult = await this.fixture.Client.PostAsJsonAsync(
            "refresh-token", new RefreshTokenRequestDto(loginResponse.RefreshToken));
        Assert.True(refreshTokenResult.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Fails_for_invalid_refresh_token()
    {
        var refreshTokenResult = await this.fixture.Client.PostAsJsonAsync(
            "refresh-token", new RefreshTokenRequestDto("some invalid refresh token"));
        Assert.False(refreshTokenResult.IsSuccessStatusCode);
    }

    private async Task<TokenResponseDto> RegisterAndLoginUser()
    {
        UserCredentialsDto credentials = new("user3@test.ro", "Password1");
        var registrationResult = await this.fixture.Client.PostAsJsonAsync(
            "register", credentials);
        Assert.True(registrationResult.IsSuccessStatusCode);

        var loginResult = await this.fixture.Client.PostAsJsonAsync(
            "login", credentials);
        Assert.True(loginResult.IsSuccessStatusCode);

        var body = await loginResult.Content.ReadFromJsonAsync<TokenResponseDto>();
        Assert.NotNull(body);
        return body;
    }
}
