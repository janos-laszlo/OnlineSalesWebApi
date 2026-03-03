using UserIdentity.Commands;

namespace SalesWebApi.IntegrationTests.UserIdentityEndpoints;

[Collection(UserIdentityFixture.CollectionName)]
public sealed class UpdateUserProfileTests(UserIdentityFixture fixture)
{
    [Theory]
    [InlineData(null, null, true, "12345678", "Test Company", "J23/2025",
        "str. principala", "Mures", "Pasareni", new string[] { "1234567890" })]
    [InlineData("John", "Doe", false, null, null, null,
        null, null, null, new string[] { "1234567890" })]
    public async Task Succeeds_for_authenticated_user_and_regular_or_dealer_profile(
        string? firstName,
        string? lastName,
        bool? isDealer,
        string? cui,
        string? companyName,
        string? registrationNumber,
        string? address,
        string? county,
        string? locality,
        string?[] phoneNumbers)
    {
        // Arrange
        string email = UserUtils.NextEmail;
        var tokenResponse = await fixture.RegisterAndLoginUser(
            new UserCredentialsDto(email, "Password1"));

        var updateProfileRequest = new UserProfileRequestDto(
            email,
            firstName,
            lastName,
            isDealer,
            cui,
            companyName,
            registrationNumber,
            address,
            county,
            locality,
            phoneNumbers);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, UserIdentityUris.ProfileUri);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", tokenResponse.AccessToken);
        request.Content = JsonContent.Create(updateProfileRequest);

        var result = await fixture.Client.SendAsync(request);

        // Assert
        Assert.True(result.IsSuccessStatusCode);
    }

    [Theory]
    [InlineData(null, null, false, "12345678", "Test Company", "J23/2025",
        "str. principala", "Mures", "Pasareni", new string[] { "1234567890" })]
    [InlineData("John", "Doe", true, null, null, null,
        null, null, null, new string[] { "1234567890" })]
    public async Task Fails_for_authenticated_user_and_invalid_profile_data(
        string? firstName,
        string? lastName,
        bool? isDealer,
        string? cui,
        string? companyName,
        string? registrationNumber,
        string? address,
        string? county,
        string? locality,
        string?[] phoneNumbers)
    {
        // Arrange
        string email = UserUtils.NextEmail;
        var tokenResponse = await fixture.RegisterAndLoginUser(
            new UserCredentialsDto(email, "Password1"));

        var updateProfileRequest = new UserProfileRequestDto(
            email,
            firstName,
            lastName,
            isDealer,
            cui,
            companyName,
            registrationNumber,
            address,
            county,
            locality,
            phoneNumbers);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, UserIdentityUris.ProfileUri);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", tokenResponse.AccessToken);
        request.Content = JsonContent.Create(updateProfileRequest);

        var result = await fixture.Client.SendAsync(request);

        // Assert
        Assert.False(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Fails_for_unauthenticated_user()
    {
        // Arrange
        var updateProfileRequest = new UserProfileRequestDto(
            Email: UserUtils.NextEmail,
            FirstName: "John",
            LastName: "Doe",
            IsDealer: null,
            Cui: null,
            CompanyName: null,
            RegistrationNumber: null,
            Address: null,
            County: null,
            Locality: null,
            PhoneNumbers: ["1234567890"]
        );

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, UserIdentityUris.ProfileUri)
        { Content = JsonContent.Create(updateProfileRequest) };

        var result = await fixture.Client.SendAsync(request);

        // Assert
        Assert.False(result.IsSuccessStatusCode);
    }
}
