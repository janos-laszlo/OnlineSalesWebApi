using System.Net.Http.Headers;
using System.Net.Http.Json;
using ObjectUploadTracking;

namespace SalesWebApi.IntegrationTests.VehicleSalesEndpoints;

[Collection(VehicleSalesFixture.CollectionName)]
public sealed class CreateVehicleSaleTests
{
    private readonly VehicleSalesFixture fixture;
    private readonly VerifySettings settings = new();

    public CreateVehicleSaleTests(VehicleSalesFixture fixture)
    {
        settings.ScrubLinesContaining("Authorization");
        settings.ScrubMember("entityId");
        settings.ScrubMember("Location");
        settings.ScrubMember("traceId");
        settings.ScrubMember("objectUploadId");
        settings.ScrubMember("objectKeysAndTheirPresignedUploadUrls");
        this.fixture = fixture;
    }

    [Fact]
    public async Task Created_for_valid_request()
    {
        // Arrange
        var requestWithoutPhotos = """
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
          "averageFuelConsumptionInLitersPer100Km": 6,
          "averageBatteryConsumptionInKWhPer100Km": 90,
          "massInKg": 1520,
          "maximumLoadInKg": 480
        }
        """;
        HttpRequestMessage httpRequest = fixture.CreateVehicleSaleRequest(requestWithoutPhotos);

        // Act
        var response = await fixture.Client.SendAsync(httpRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ObjectUploadTrackingDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Null(result.ObjectUploadId);
        Assert.Null(result.ObjectKeysAndTheirPresignedUploadUrls);
    }

    [Fact]
    public async Task BadRequest_for_missing_required_fields()
    {
        // Arrange
        var requestWithoutRequiredFields =
            """
            {
              "numberOfPreviousOwners": 1,
              "batteryCapacityInKWh": 11000,
              "rangeInKilometers": 1000,
              "averageFuelConsumptionInLitersPer100Km": 6,
              "averageBatteryConsumptionInKWhPer100Km": 90,
              "massInKg": 1520,
              "maximumLoadInKg": 480
            }
            """;
        var httpRequest = fixture.CreateVehicleSaleRequest(requestWithoutRequiredFields);

        // Act
        var response = await fixture.Client.SendAsync(httpRequest, TestContext.Current.CancellationToken);

        // Assert
        await Verify(response, settings);
    }


    [Fact]
    public async Task Created_for_sale_with_photos_and_confirmed_upload()
    {
        // Arrange
        var requestWithoutRequiredFields =
            """
            {
              "title": "2019 BMW 3 Series - Excellent Condition",
              "description": "Well maintained 2019 BMW 3 Series with full service history. One previous owner, no accidents. Comes with winter tires and original floor mats.",
              "amountInCents": 2499900,
              "currency": "EUR",
              "county": "Los Angeles",
              "locality": "Santa Monica",
              "vehicleModelId": 62,
              "mileageInKilometers": 45000,
              "horsePower": 255,
              "photoContentTypes": ["image/jpeg", "image/png", "image/jpeg"]
            }
            """;
        var httpRequest = fixture.CreateVehicleSaleRequest(requestWithoutRequiredFields);

        // Act
        var response = await fixture.Client.SendAsync(httpRequest, TestContext.Current.CancellationToken);
        Assert.True(response.IsSuccessStatusCode);

        var content = await response.Content.ReadFromJsonAsync<ObjectUploadTrackingDto>(
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(3, content?.ObjectKeysAndTheirPresignedUploadUrls?.Count);

        var filesToUpload = new Queue<(string FilePath, string ContentType)>();
        var photosDirectory = Path.Combine(AppContext.BaseDirectory, "VehicleSalesEndpoints", "Data");
        filesToUpload.Enqueue((Path.Combine(photosDirectory, "sample1.jpg"), "image/jpeg"));
        filesToUpload.Enqueue((Path.Combine(photosDirectory, "sample2.png"), "image/png"));
        filesToUpload.Enqueue((Path.Combine(photosDirectory, "sample3.jpg"), "image/jpeg"));

        foreach (var objectKeyAndPresignedUrl in content!.ObjectKeysAndTheirPresignedUploadUrls!)
        {
            var (FilePath, ContentType) = filesToUpload.Dequeue();
            var fileBytes = File.ReadAllBytes(FilePath);
            var byteContent = new ByteArrayContent(fileBytes);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue(ContentType);

            var putResponse = await fixture.ExternalClient.PutAsync(
                objectKeyAndPresignedUrl.Value,
                byteContent,
                TestContext.Current.CancellationToken);
            Assert.True(putResponse.IsSuccessStatusCode);
        }

        var confirmRequest = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{VehicleSalesUris.ConfirmObjectUpload}{content.ObjectUploadId}");
        confirmRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", fixture.AccessToken);

        var confirmResponse = await fixture.Client.SendAsync(
            confirmRequest,
            TestContext.Current.CancellationToken);
        Assert.True(confirmResponse.IsSuccessStatusCode);
    }
}
