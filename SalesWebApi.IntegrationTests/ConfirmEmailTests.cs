using UserIdentity.Commands;

namespace SalesWebApi.IntegrationTests;

[Collection("User Identity")]
public sealed class ConfirmEmailTests(UserIdentityFixture fixture)
{
    [Fact]
    public async Task Fails_for_invalid_confirmation_token()
    {
        // Arrange

        // Act
        var result = await fixture.Client.GetAsync(
            Endpoints.ConfirmEmailUri + "some-invalid-token-that-doesnt-exist");

        // Assert
        Assert.False(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Succeeds_for_valid_confirmation_token()
    {
        // Arrange
        await fixture.Client.PostAsJsonAsync(
            Endpoints.RegisterUri, new UserCredentialsDto(UserUtils.NextEmail, "Password1"));

        // Act
        string confirmationToken = ExtractConfirmationToken(
            fixture.EmailService.Emails.First().Body);
        var result = await fixture.Client.GetAsync(
            $"{Endpoints.ConfirmEmailUri}{confirmationToken}");

        // Assert
        Assert.True(result.IsSuccessStatusCode);
    }

    private static string ExtractConfirmationToken(string body)
    {
        int startIndex = body.IndexOf("token=") + 6;
        int endIndex = body.IndexOf(Environment.NewLine, startIndex);
        return body[startIndex..endIndex];
    }
}
