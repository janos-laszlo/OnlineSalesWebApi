using UserIdentity.Commands;

namespace SalesWebApi.IntegrationTests;

[Collection("User Identity")]
public sealed class UserLoginTests(UserIdentityFixture fixture)
{
    [Fact]
    public async Task Succeeds_for_existing_credentials()
    {
        // Arrange
        UserCredentialsDto credentials = new(UserUtils.NextEmail, "Password1");

        // Act
        var registrationResult = await fixture.Client.PostAsJsonAsync(
            "register", credentials);
        Assert.True(registrationResult.IsSuccessStatusCode);

        var result = await fixture.Client.PostAsJsonAsync(
            "login", credentials);
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Fails_for_inexistent_email()
    {
        // Arrange
        UserCredentialsDto credentials = new(UserUtils.NextEmail, "Password1");

        // Act
        var result = await fixture.Client.PostAsJsonAsync(
            "login", credentials);
        Assert.False(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Fails_for_wrong_password()
    {
        // Arrange
        UserCredentialsDto credentials = new(UserUtils.NextEmail, "Password1");

        // Act
        var registrationResult = await fixture.Client.PostAsJsonAsync(
            "register", credentials);
        Assert.True(registrationResult.IsSuccessStatusCode);

        var result = await fixture.Client.PostAsJsonAsync(
            "login", credentials with { Password = "WrongPassword2"});
        Assert.False(result.IsSuccessStatusCode);
    }
}
