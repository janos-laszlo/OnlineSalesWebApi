using System.Net.Http.Headers;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using UserIdentity.Commands;

namespace SalesWebApi.IntegrationTests;

[Collection("User Identity")]
public sealed class LoginUserTests(UserIdentityFixture fixture)
{
    [Fact]
    public async Task Succeeds_for_existing_credentials()
    {
        // Arrange
        UserCredentialsDto credentials = new(UserUtils.NextEmail, "Password1");

        // Act
        var registrationResult = await fixture.Client.PostAsJsonAsync(
            Endpoints.RegisterUri, credentials);
        Assert.True(registrationResult.IsSuccessStatusCode);

        var result = await fixture.Client.PostAsJsonAsync(
            Endpoints.LoginUri, credentials);
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Fails_for_inexistent_email()
    {
        // Arrange
        UserCredentialsDto credentials = new(UserUtils.NextEmail, "Password1");

        // Act
        var result = await fixture.Client.PostAsJsonAsync(
            Endpoints.LoginUri, credentials);
        Assert.False(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Fails_for_wrong_password()
    {
        // Arrange
        UserCredentialsDto credentials = new(UserUtils.NextEmail, "Password1");

        // Act
        var registrationResult = await fixture.Client.PostAsJsonAsync(
            Endpoints.RegisterUri, credentials);
        Assert.True(registrationResult.IsSuccessStatusCode);

        var result = await fixture.Client.PostAsJsonAsync(
            Endpoints.LoginUri, credentials with { Password = "WrongPassword2"});
        Assert.False(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Fails_when_JWT_is_tampered_with()
    {
        // Arrange
        UserCredentialsDto credentials = new(UserUtils.NextEmail, "Password1");
        var tokenResponse = await RegisterAndLoginUser(credentials);

        // Act
        var statusCode = await GetHealthCheckWithToken(tokenResponse.AccessToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, statusCode);
        
        // Tamper with the token
        var token = CreateAccessedTokenWithIncorrectSignatureKey("1", credentials.Email);
        // Try to use the tampered token for a protected endpoint
        var protectedResult = await GetHealthCheckWithToken(token);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, protectedResult);
    }
    
    private async Task<TokenResponseDto> RegisterAndLoginUser(UserCredentialsDto credentials)
    {
        var registrationResult = await fixture.Client.PostAsJsonAsync(
            Endpoints.RegisterUri, credentials);
        Assert.True(registrationResult.IsSuccessStatusCode);

        var loginResult = await fixture.Client.PostAsJsonAsync(
            Endpoints.LoginUri, credentials);
        Assert.True(loginResult.IsSuccessStatusCode);

        var body = await loginResult.Content.ReadFromJsonAsync<TokenResponseDto>();
        Assert.NotNull(body);
        return body;
    }
    
    private static string CreateAccessedTokenWithIncorrectSignatureKey(string userId, string email)
    {
        var data = Encoding.UTF8.GetBytes("some incorrect very_secret key h4iot-");
        var securityKey = new SymmetricSecurityKey(data);

        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = userId,
            [JwtRegisteredClaimNames.Email] = email
        };
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "http://localhost:1234",
            Audience = "http://localhost:1234",
            Claims = claims,
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(120),
            SigningCredentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256)
        };

        var jwtHandler = new JsonWebTokenHandler();
        return jwtHandler.CreateToken(descriptor);
    }

    private async Task<System.Net.HttpStatusCode> GetHealthCheckWithToken(string accessToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, Endpoints.HealthUri);
        requestMessage.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        using var responseMessage = await fixture.Client.SendAsync(requestMessage);
        return responseMessage.StatusCode;
    }
}
