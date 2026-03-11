using System.Net.Http.Json;
using UserIdentity.Commands;

namespace SalesWebApi.IntegrationTests.UserIdentityEndpoints;

[Collection(UserIdentityFixture.CollectionName)]
public sealed class ConfirmEmailTests(UserIdentityFixture fixture)
{
    [Fact]
    public async Task Fails_for_invalid_confirmation_token()
    {
        // Arrange

        // Act
        var result = await fixture.Client.GetAsync(
            UserIdentityUris.ConfirmEmailUri + "some-invalid-token-that-doesnt-exist",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Succeeds_for_valid_confirmation_token()
    {
        // Arrange
        await fixture.Client.PostAsJsonAsync(
            UserIdentityUris.RegisterUri,
            new UserCredentialsDto(UserUtils.NextEmail, "Password1"),
            TestContext.Current.CancellationToken);

        // Act
        string confirmationToken = ExtractConfirmationToken(
            fixture.EmailService.Emails.First().Body);
        var result = await fixture.Client.GetAsync(
            $"{UserIdentityUris.ConfirmEmailUri}{confirmationToken}",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Succeeds_for_already_confirmed_email()
    {
        // Arrange
        string email = UserUtils.NextEmail;
        var response = await fixture.Client.PostAsJsonAsync(
            UserIdentityUris.RegisterUri,
            new UserCredentialsDto(email, "Password1"), TestContext.Current.CancellationToken);
        Assert.True(response.IsSuccessStatusCode);

        string confirmationToken = ExtractConfirmationToken(
            fixture.EmailService.Emails.First().Body);
        await fixture.Client.GetAsync(
            $"{UserIdentityUris.ConfirmEmailUri}{confirmationToken}", TestContext.Current.CancellationToken);

        // Act
        var result = await fixture.Client.GetAsync(
            $"{UserIdentityUris.ConfirmEmailUri}{confirmationToken}", TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Fails_for_token_encrypted_with_wrong_purpose()
    {
        // Arrange
        string email = UserUtils.NextEmail;
        await fixture.Client.PostAsJsonAsync(
            UserIdentityUris.RegisterUri,
            new UserCredentialsDto(email, "Password1"),
            TestContext.Current.CancellationToken);

        string confirmationTokenWithDifferentPurposeKey = fixture
            .AnotherEmailConfirmationToken
            .GenerateToken(1, email);

        // Act
        var result = await fixture.Client.GetAsync(
            $"{UserIdentityUris.ConfirmEmailUri}{confirmationTokenWithDifferentPurposeKey}",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccessStatusCode);
    }

    private static string ExtractConfirmationToken(string body)
    {
        int startIndex = body.IndexOf("token=") + 6;
        int endIndex = body.IndexOf(Environment.NewLine, startIndex);
        return body[startIndex..endIndex];
    }
}
