using Microsoft.AspNetCore.Mvc.Testing;
using UserIdentity.Commands;

namespace SalesWebApi.IntegrationTests;

public sealed class UserRegistrationTests : IClassFixture<UserRegistrationFixture>
{
    private readonly UserRegistrationFixture fixture;

    public UserRegistrationTests(UserRegistrationFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task Succeeds_for_valid_email_and_password()
    {
        // Arrange

        // Act
        var result = await fixture.Client.PostAsJsonAsync(
            "register", 
            new UserCredentialsDto("test@example.com", "SecurePassword123!"));

        // Assert
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Not_allowed_for_invalid_email()
    {
        // Arrange

        // Act
        var result = await fixture.Client.PostAsJsonAsync(
            "register", 
            new UserCredentialsDto("invalid-email", "SecurePassword123!"));

        // Assert
        Assert.False(result.IsSuccessStatusCode);
    }

    [Theory]
    [InlineData("lowercaseonly")]
    [InlineData("UPPERCASEONLY")]
    [InlineData("NoNumbers!")]
    [InlineData("Short1")]
    public async Task Not_allowed_for_invalid_password(string invalidPassword)
    {
        // Arrange

        // Act
        var result = await fixture.Client.PostAsJsonAsync(
            "register", 
            new UserCredentialsDto("test@example.com", invalidPassword));

        // Assert
        Assert.False(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Not_allowed_for_existing_email()
    {
        // Arrange

        // Act & Assert
        var result = await fixture.Client.PostAsJsonAsync(
            "register", 
            new UserCredentialsDto("test1@gmail.com", "SecurePassword123!"));

        Assert.True(result.IsSuccessStatusCode);
        
        result = await fixture.Client.PostAsJsonAsync(
            "register", 
            new UserCredentialsDto("test1@gmail.com", "Password123!"));

        Assert.False(result.IsSuccessStatusCode);
    }
}
