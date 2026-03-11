using System.Net.Http.Json;
using UserIdentity.Commands;

namespace SalesWebApi.IntegrationTests.UserIdentityEndpoints;

[Collection(UserIdentityFixture.CollectionName)]
public sealed class RegisterUserTests(UserIdentityFixture fixture)
{
    [Fact]
    public async Task Succeeds_for_valid_email_and_password()
    {
        // Arrange

        // Act
        var result = await fixture.Client.PostAsJsonAsync(
            UserIdentityUris.RegisterUri,
            new UserCredentialsDto(UserUtils.NextEmail, "SecurePassword123!"),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Fails_for_invalid_email()
    {
        // Arrange

        // Act
        var result = await fixture.Client.PostAsJsonAsync(
            UserIdentityUris.RegisterUri,
            new UserCredentialsDto("invalid-email", "SecurePassword123!"),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccessStatusCode);
    }

    [Theory]
    [InlineData("lowercaseonly")]
    [InlineData("UPPERCASEONLY")]
    [InlineData("NoNumbers!")]
    [InlineData("Short1")]
    public async Task Fails_for_invalid_password(string invalidPassword)
    {
        // Arrange

        // Act
        var result = await fixture.Client.PostAsJsonAsync(
            UserIdentityUris.RegisterUri,
            new UserCredentialsDto(UserUtils.NextEmail, invalidPassword),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Fails_for_existing_email()
    {
        // Arrange

        // Act & Assert
        string email = UserUtils.NextEmail;
        var result = await fixture.Client.PostAsJsonAsync(
            UserIdentityUris.RegisterUri,
            new UserCredentialsDto(email, "SecurePassword123!"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccessStatusCode);

        result = await fixture.Client.PostAsJsonAsync(
            UserIdentityUris.RegisterUri,
            new UserCredentialsDto(email, "Password123!"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccessStatusCode);
    }
}
