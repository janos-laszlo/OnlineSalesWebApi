using ObjectUploadTracking;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace SalesWebApi.IntegrationTests.VehicleSalesEndpoints;

[Collection(VehicleSalesFixture.CollectionName)]
public sealed class UpdateVehicleSaleTests
{
    private readonly VehicleSalesFixture _fixture;
    private readonly VerifySettings _verifySettings;

    public UpdateVehicleSaleTests(
        VehicleSalesFixture fixture)
    {
        _fixture = fixture;
        _verifySettings = new VerifySettings();
        _verifySettings.ScrubMember("Id");
        _verifySettings.ScrubMember("SellerId");
    }

    [Fact]
    public async Task WithValidData_ReturnsOkAndUpdatesSale()
    {
        // Arrange
        var vehicleSaleId = await _fixture.CreateDefaultVehicleSaleAsync();
        var existingSale = await _fixture.GetVehicleSaleAsync(vehicleSaleId);
        Assert.NotNull(existingSale); // Ensure the sale was created successfully

        var updatedVehicleSale =
        """
        {
            "title": "2019 BMW 3 Series - Very Good Condition",
            "description": "Very well maintained 2019 BMW 3 Series with full service history. One previous owner, no accidents. Comes with winter tires and original floor mats.",
            "amountInCents": 2699900,
            "currency": "RON",
            "county": "San Francisco",
            "locality": "Santa Monica1",
            "vehicleModelId": 69,
            "mileageInKilometers": 55000,
            "horsePower": 257,
            "vehicleVersion": "High Line",
            "bodyType": "Coupe",
            "engineVolumeInCm3": 1999,
            "exteriorColor": "Alpine Black",
            "interiorColor": "White",
            "fuelType": "Diesel",
            "vehicleManufacturingYear": 2020,
            "vehicleNumberOfDoors": 5,
            "vehicleCondition": "New",
            "gearboxType": "Manual",
            "steeringWheelSide": "Right",
            "driveType": "RearWheelDrive",
            "numberOfSeats": 5,
            "emissionStandard": "EURO7",
            "hasServiceHistory": true,
            "hasAccidentHistory": true,
            "vin": "WBA5R1C50KAK54321",
            "numberOfPreviousOwners": 2,
            "batteryCapacityInKWh": 1000,
            "rangeInKilometers": 1100,
            "averageFuelConsumptionInLitersPer100Km": 7
        }
        """;
        var updateRequest = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{Endpoints.VehicleSalesEndpoints.VehicleSalesBase}/{existingSale.Id}")
        {
            Content = new StringContent(updatedVehicleSale, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", _fixture.AccessToken);

        // Act
        var response = await _fixture.Client.SendAsync(
            updateRequest,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updateResponse = await response.Content.ReadFromJsonAsync<ObjectUploadTrackingDto>(
            TestContext.Current.CancellationToken);
        Assert.NotNull(updateResponse);
        Assert.Equal(existingSale.Id, updateResponse?.EntityId);

        var updatedSale = await _fixture.GetVehicleSaleAsync(existingSale.Id);

        await Verify(updatedSale, _verifySettings);
    }

    [Fact]
    public async Task WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var vehicleSaleId = await _fixture.CreateDefaultVehicleSaleAsync();
        var existingSale = await _fixture.GetVehicleSaleAsync(vehicleSaleId);
        Assert.NotNull(existingSale); // Ensure the sale was created successfully

        var invalidRequest = """
        {        
          "title": "short title",
          "description": "short desc",
          "amountInCents": -2499900,
          "currency": "USD",
          "county": "",
          "locality": "",
          "vehicleModelId": 68000,
          "mileageInKilometers": -45000,
          "horsePower": -255,
          "vehicleVersion": "",
          "bodyType": "BLA",
          "engineVolumeInCm3": -1998,
          "exteriorColor": "",
          "interiorColor": "",
          "fuelType": "bLA",
          "vehicleManufacturingYear": 219,
          "vehicleNumberOfDoors": 14,
          "vehicleCondition": "UNUsed",
          "gearboxType": "HYBRID",
          "steeringWheelSide": "BACK",
          "driveType": "rwd",
          "numberOfSeats": -50,
          "emissionStandard": "EURO60",
          "hasServiceHistory": true,
          "hasAccidentHistory": false,
          "vin": "WBA5R1C50KAK12345234",
          "numberOfPreviousOwners": -1,
          "batteryCapacityInKWh": -11000,
          "rangeInKilometers": -1000,
          "averageFuelConsumptionInLitersPer100Km": 6
        }
        """;

        var updateRequest = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{Endpoints.VehicleSalesEndpoints.VehicleSalesBase}/{existingSale.Id}")
        {
            Content = new StringContent(invalidRequest, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", _fixture.AccessToken);

        // Act
        var response = await _fixture.Client.SendAsync(
            updateRequest,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task WithNonExistentVehicleSale_ReturnsBadRequest()
    {
        // Arrange
        var updatedVehicleSale =
        """
        {
            "title": "2019 BMW 3 Series - Very Good Condition",
            "description": "Very well maintained 2019 BMW 3 Series with full service history. One previous owner, no accidents. Comes with winter tires and original floor mats.",
            "amountInCents": 2699900,
            "currency": "RON",
            "county": "San Francisco",
            "locality": "Santa Monica1",
            "vehicleModelId": 69
        }
        """;
        var updateRequest = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{Endpoints.VehicleSalesEndpoints.VehicleSalesBase}/{1000000}") // Non-existent ID
        {
            Content = new StringContent(updatedVehicleSale, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", _fixture.AccessToken);

        // Act
        var response = await _fixture.Client.SendAsync(
            updateRequest,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AnotherUsersSale_ReturnsBadRequest()
    {
        // Arrange
        var updatedVehicleSale =
        """
        {
            "title": "2019 BMW 3 Series - Very Good Condition",
            "description": "Very well maintained 2019 BMW 3 Series with full service history. One previous owner, no accidents. Comes with winter tires and original floor mats.",
            "amountInCents": 2699900,
            "currency": "RON",
            "county": "San Francisco",
            "locality": "Santa Monica1",
            "vehicleModelId": 69
        }
        """;
        // Assume _fixture.AnotherUsersVehicleSaleId is a valid sale owned by another user
        var updateRequest = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{Endpoints.VehicleSalesEndpoints.VehicleSalesBase}/{_fixture.AnotherUsersVehicleSaleId.Value}")
        {
            Content = new StringContent(updatedVehicleSale, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", _fixture.AccessToken);

        // Act
        var response = await _fixture.Client.SendAsync(
            updateRequest,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task WithoutAuthHeader_ReturnsUnauthorized()
    {
        // Arrange
        var vehicleSaleId = await _fixture.CreateDefaultVehicleSaleAsync();
        var updatedVehicleSale =
        """
        {
            "title": "2019 BMW 3 Series - Very Good Condition",
            "description": "Very well maintained 2019 BMW 3 Series with full service history. One previous owner, no accidents. Comes with winter tires and original floor mats.",
            "amountInCents": 2699900,
            "currency": "RON",
            "county": "San Francisco",
            "locality": "Santa Monica1",
            "vehicleModelId": 69
        }
        """;
        // Assume _fixture.VehicleSaleId is a valid sale owned by the test user
        var updateRequest = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{Endpoints.VehicleSalesEndpoints.VehicleSalesBase}/{vehicleSaleId}")
        {
            Content = new StringContent(updatedVehicleSale, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        // No Authorization header

        // Act
        var response = await _fixture.Client.SendAsync(
            updateRequest,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    // No update on photos
    [InlineData("[\"image/jpeg\", \"image/png\"]", null, "[\"0.jpeg\",\"1.png\"]")]
    // Reorder photos
    [InlineData("[\"image/jpeg\", \"image/png\"]", "[\"1.png\", \"0.jpeg\"]", "[\"1.png\",\"0.jpeg\"]")]
    // Remove 1 photo
    [InlineData("[\"image/jpeg\", \"image/png\", \"image/jpeg\"]", "[\"0.jpeg\", \"2.jpeg\"]", "[\"0.jpeg\",\"2.jpeg\"]")]
    // Add 1 photo
    [InlineData("[\"image/jpeg\", \"image/jpeg\"]", "[\"0.jpeg\", \"image/png\", \"1.jpeg\"]", "[\"0.jpeg\",\"2.png\",\"1.jpeg\"]")]
    // Add 1 photo and reorder
    [InlineData("[\"image/jpeg\", \"image/jpeg\"]", "[\"1.jpeg\", \"image/png\", \"0.jpeg\"]", "[\"1.jpeg\",\"2.png\",\"0.jpeg\"]")]
    // Remove 1 photo and reorder
    [InlineData("[\"image/jpeg\", \"image/jpeg\", \"image/png\"]", "[\"2.png\", \"0.jpeg\"]", "[\"2.png\",\"0.jpeg\"]")]
    // Remove 1 photo and add 1 photo (without reorder)
    [InlineData("[\"image/jpeg\", \"image/jpeg\", \"image/png\"]", "[\"0.jpeg\", \"image/jpeg\", \"2.png\"]", "[\"0.jpeg\",\"3.jpeg\",\"2.png\"]")]
    // Remove 1 photo, add 1 photo and reorder
    [InlineData("[\"image/jpeg\", \"image/jpeg\", \"image/png\"]", "[\"2.png\", \"0.jpeg\", \"image/jpeg\"]", "[\"2.png\",\"0.jpeg\",\"3.jpeg\"]")]
    public async Task PhotoOperations(string creationPhotoContentTypes, string? updatedPhotoKeys, string expectedPhotoKeys)
    {
        // Arrange
        var newVehicleSale =
        $$"""
        {
            "title": "2019 BMW 3 Series - Very Good Condition",
            "description": "Very well maintained 2019 BMW 3 Series with full service history. One previous owner, no accidents. Comes with winter tires and original floor mats.",
            "amountInCents": 2699900,
            "currency": "RON",
            "county": "San Francisco",
            "locality": "Santa Monica1",
            "vehicleModelId": 69,
            "photoContentTypes": {{creationPhotoContentTypes}}
        }
        """;
        var vehicleSaleId = await _fixture.CreateVehicleSaleAsync(newVehicleSale);
        await _fixture.UpdateVehicleSale(updatedPhotoKeys, vehicleSaleId);

        var afterUpdateSale = await _fixture.GetVehicleSaleAsync(vehicleSaleId);

        // Assert
        Assert.NotNull(afterUpdateSale);
        string serializedUpdatedPhotoKeys = JsonSerializer.Serialize(afterUpdateSale.PhotoKeys);
        Assert.Equal(expectedPhotoKeys, serializedUpdatedPhotoKeys);
        Assert.NotNull(afterUpdateSale.Directory);
        var objectsInDirectory = await _fixture.GetObjectsInDirectory(afterUpdateSale.Directory, TestContext.Current.CancellationToken);
        Assert.Equal(
            JsonSerializer.Deserialize<HashSet<string>>(expectedPhotoKeys),
            [.. JsonSerializer.Deserialize<List<string>>(JsonSerializer.Serialize(objectsInDirectory)) ?? []]);
    }
}
