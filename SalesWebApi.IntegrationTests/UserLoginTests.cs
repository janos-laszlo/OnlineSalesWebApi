using UserIdentity.Commands;

namespace SalesWebApi.IntegrationTests;

public sealed class UserLoginTests : IClassFixture<UserIdentityFixture>
{
    private readonly UserIdentityFixture fixture;

    public UserLoginTests(UserIdentityFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task Succeeds_for_existing_credentials()
    {
        // Arrange
        UserCredentialsDto credentials = new("user@gmail.com", "Password1");

        // Act
        var registrationResult = await this.fixture.Client.PostAsJsonAsync(
            "register", credentials);
        Assert.True(registrationResult.IsSuccessStatusCode);

        var result = await this.fixture.Client.PostAsJsonAsync(
            "login", credentials);
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Fails_for_inexistent_email()
    {
        // Arrange
        UserCredentialsDto credentials = new("inexistent@gmail.com", "Password1");

        // Act
        var result = await this.fixture.Client.PostAsJsonAsync(
            "login", credentials);
        Assert.False(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Fails_for_wrong_password()
    {
        // Arrange
        UserCredentialsDto credentials = new("user1@gmail.com", "Password1");

        // Act
        var registrationResult = await this.fixture.Client.PostAsJsonAsync(
            "register", credentials);
        Assert.True(registrationResult.IsSuccessStatusCode);

        var result = await this.fixture.Client.PostAsJsonAsync(
            "login", credentials with { Password = "WrongPassword2"});
        Assert.False(result.IsSuccessStatusCode);
    }
}
