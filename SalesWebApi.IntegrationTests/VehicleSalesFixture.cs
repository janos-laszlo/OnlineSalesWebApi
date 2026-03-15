using Microsoft.AspNetCore.Mvc.Testing;
using ObjectUploadTracking;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using UserIdentity.Commands;
using VehicleSales.Queries;

namespace SalesWebApi.IntegrationTests;

public sealed class VehicleSalesFixture : IDisposable
{
    internal const string CollectionName = "Vehicle Sales";
    private readonly WebApplicationFactory<Program> app;
    internal HttpClient Client { get; }
    internal HttpClient ExternalClient { get; }
    internal string AccessToken { get; }
    public Lazy<int> AnotherUsersVehicleSaleId { get; }

    public VehicleSalesFixture()
    {
        app = new WebApplicationFactory<Program>();
        Client = app.CreateClient();
        ExternalClient = new HttpClient();
        AccessToken = RegisterAndLoginUser(new UserCredentialsDto(UserUtils.NextEmail, "Password1")).AccessToken;
        AnotherUsersVehicleSaleId = new Lazy<int>(() =>
        {
            var otherUserToken = RegisterAndLoginUser(new UserCredentialsDto(UserUtils.NextEmail, "Password1")).AccessToken;
            var httpRequest = new HttpRequestMessage(
                HttpMethod.Post, Endpoints.VehicleSalesEndpoints.VehicleSalesBase)
            {
                Content = new StringContent(
                    """
                    {        
                      "title": "2019 Audi A4 - Excellent Condition",
                      "description": "Well maintained 2019 Audi A4 with full service history. One previous owner, no accidents. Comes with winter tires and original floor mats.",
                      "amountInCents": 2699900,
                      "currency": "EUR",
                      "county": "Los Angeles",
                      "locality": "Santa Monica",
                      "vehicleModelId": 70,
                      "mileageInKilometers": 40000,
                      "horsePower": 248,
                      "vehicleVersion": "Premium Line",
                      "bodyType": "Sedan",
                      "engineVolumeInCm3": 1984,
                      "exteriorColor": "Mythos Black Metallic",
                      "interiorColor": "Gray",
                      "fuelType": "Petrol",
                      "vehicleManufacturingYear": 2019,
                      "vehicleNumberOfDoors": 4,
                      "vehicleCondition": "Used",
                      "gearboxType": "Automatic",
                      "steeringWheelSide": "Left",
                      "driveType": "FrontWheelDrive",
                      "numberOfSeats": 5,
                      "emissionStandard": "EURO6",
                      "hasServiceHistory": true,
                      "hasAccidentHistory": false,
                      "vin": "WAUENAF48KN123456",
                      "numberOfPreviousOwners": 1,
                      "batteryCapacityInKWh": 0,
                      "rangeInKilometers": 0,
                      "averageFuelConsumptionInLitersPer100Km": 7
                    }
                    """, Encoding.UTF8, MediaTypeNames.Application.Json)
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", otherUserToken);
            var response = Client.SendAsync(httpRequest, TestContext.Current.CancellationToken).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var location = response.Headers.Location?.OriginalString;
            Assert.NotNull(location);
            var idString = location.Split('/').Last();
            return int.Parse(idString);
        });
    }

    public void Dispose()
    {
        Client.Dispose();
        ExternalClient.Dispose();
        app.Dispose();
    }

    private TokenResponseDto RegisterAndLoginUser(UserCredentialsDto credentials)
    {
        var registrationResult = this.Client
            .PostAsJsonAsync(UserIdentityUris.RegisterUri, credentials)
            .GetAwaiter()
            .GetResult();
        Assert.True(registrationResult.IsSuccessStatusCode);

        var loginResult = this.Client
            .PostAsJsonAsync(UserIdentityUris.LoginUri, credentials)
            .GetAwaiter()
            .GetResult();
        Assert.True(loginResult.IsSuccessStatusCode);

        var body = loginResult.Content.ReadFromJsonAsync<TokenResponseDto>()
            .GetAwaiter()
            .GetResult();
        Assert.NotNull(body);
        return body;
    }


    internal Task<VehicleSaleDto?> GetVehicleSaleAsync(int id) =>
        Client.GetFromJsonAsync<VehicleSaleDto>($"{VehicleSalesUris.GetVehicleSaleById}{id}");

    internal async Task<int> CreateDefaultVehicleSaleAsync()
    {
        var requestBody =
        """
        {        
          "title": "2019 BMW 3 Series - Excellent Condition",
          "description": "Well maintained 2019 BMW 3 Series with full service history. One previous owner, no accidents. Comes with winter tires and original floor mats.",
          "amountInCents": 2499900,
          "currency": "EUR",
          "county": "Los Angeles",
          "locality": "Santa Monica",
          "vehicleModelId": 68,
          "mileageInKilometers": 45000,
          "horsePower": 255,
          "vehicleVersion": "Sport Line",
          "bodyType": "Sedan",
          "engineVolumeInCm3": 1998,
          "exteriorColor": "Alpine White",
          "interiorColor": "Black",
          "fuelType": "Petrol",
          "vehicleManufacturingYear": 2019,
          "vehicleNumberOfDoors": 4,
          "vehicleCondition": "Used",
          "gearboxType": "Automatic",
          "steeringWheelSide": "Left",
          "driveType": "RearWheelDrive",
          "numberOfSeats": 5,
          "emissionStandard": "EURO6",
          "hasServiceHistory": true,
          "hasAccidentHistory": false,
          "vin": "WBA5R1C50KAK12345",
          "numberOfPreviousOwners": 1,
          "batteryCapacityInKWh": 11000,
          "rangeInKilometers": 1000,
          "averageFuelConsumptionInLitersPer100Km": 6
        }
        """;

        return await CreateVehicleSaleAsync(requestBody);
    }

    internal async Task<int> CreateVehicleSaleAsync(string requestBody)
    {
        var httpRequest = CreateVehicleSaleRequest(requestBody);
        var response = await Client.SendAsync(httpRequest, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location?.OriginalString;
        Assert.NotNull(location);
        var idString = location.Split('/').Last();

        if (requestBody.Contains("photoContentTypes"))
        {
            var responseBody = await response.Content.ReadFromJsonAsync<ObjectUploadTrackingDto>(TestContext.Current.CancellationToken);
            var objectUploadId = await UploadPhotos(responseBody);
            await ConfirmObjectUpload(objectUploadId);
        }

        return int.Parse(idString);
    }

    private async Task<int> UploadPhotos(ObjectUploadTrackingDto? responseBody)
    {
        Assert.NotNull(responseBody);
        Assert.True(responseBody.ObjectUploadId.HasValue);
        var presignedUrls = responseBody.ObjectKeysAndTheirPresignedUploadUrls;
        Assert.NotNull(presignedUrls);
        foreach (var presignedUrl in presignedUrls.Values)
        {
            var uploadResponse = await ExternalClient.PutAsync(presignedUrl, new ByteArrayContent([]), TestContext.Current.CancellationToken);
            uploadResponse.EnsureSuccessStatusCode();
        }

        return responseBody.ObjectUploadId.Value;
    }

    private async Task ConfirmObjectUpload(int objectUploadId)
    {
        var confirmRequest = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{VehicleSalesUris.ConfirmObjectUpload}{objectUploadId}");
        confirmRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", this.AccessToken);

        var confirmResponse = await Client.SendAsync(
            confirmRequest,
            TestContext.Current.CancellationToken);
        Assert.True(confirmResponse.IsSuccessStatusCode);
    }

    internal HttpRequestMessage CreateVehicleSaleRequest(string requestBody)
    {
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post, Endpoints.VehicleSalesEndpoints.VehicleSalesBase)
        {
            Content = new StringContent(requestBody, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", AccessToken);
        return httpRequest;
    }
}

[CollectionDefinition(VehicleSalesFixture.CollectionName)]
public class VehicleSalesCollection : ICollectionFixture<VehicleSalesFixture>
{ }
