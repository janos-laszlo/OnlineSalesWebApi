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

        var updateRequest = _fixture.BuildUpdateRequest(existingSale.Id,
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
        """);

        // Act
        var response = await _fixture.Client.SendAsync(
            updateRequest,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

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

        var invalidRequest = _fixture.BuildUpdateRequest(existingSale.Id,
        """
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
        """);

        // Act
        var response = await _fixture.Client.SendAsync(
            invalidRequest,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task WithNonExistentVehicleSale_ReturnsBadRequest()
    {
        // Arrange
        var updateRequest = _fixture.BuildUpdateRequest(1000000,
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
        """);

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
        // Assume _fixture.AnotherUsersVehicleSaleId is a valid sale owned by another user
        var updateRequest = _fixture.BuildUpdateRequest(_fixture.AnotherUsersVehicleSaleId.Value,
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
        """);

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
        var updateRequest = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{Endpoints.VehicleSalesEndpoints.VehicleSalesBase}/{vehicleSaleId}")
        {
            Content = new MultipartFormDataContent
            {
                { new StringContent("San Francisco"), "county" },
                { new StringContent("Santa Monica1"), "locality" },
                { new StringContent("69"), "vehicleModelId" },
            }
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
    [InlineData(2, null, 0, "[\"0.jpeg\",\"1.png\"]")]
    // Reorder photos
    [InlineData(2, "[\"1.png\",\"0.jpeg\"]", 0, "[\"1.png\",\"0.jpeg\"]")]
    // Remove 1 photo
    [InlineData(3, "[\"0.jpeg\",\"2.jpeg\"]", 0, "[\"0.jpeg\",\"2.jpeg\"]")]
    // Add 1 photo
    [InlineData(2, "[\"0.jpeg\",\"1.png\"]", 1, "[\"0.jpeg\",\"1.png\",\"2.jpeg\"]")]
    // Add 1 photo and reorder
    [InlineData(2, "[\"1.png\",\"0.jpeg\"]", 1, "[\"1.png\",\"0.jpeg\",\"2.jpeg\"]")]
    // Remove 1 photo and reorder
    [InlineData(3, "[\"2.jpeg\",\"0.jpeg\"]", 0, "[\"2.jpeg\",\"0.jpeg\"]")]
    // Remove 1 photo and add 1 photo (without reorder)
    [InlineData(3, "[\"0.jpeg\",\"2.jpeg\"]", 1, "[\"0.jpeg\",\"2.jpeg\",\"3.jpeg\"]")]
    // Remove 1 photo, add 1 photo and reorder
    [InlineData(3, "[\"2.jpeg\",\"0.jpeg\"]", 1, "[\"2.jpeg\",\"0.jpeg\",\"3.jpeg\"]")]
    public async Task PhotoOperations(int creationFileCount, string? existingPhotosJson, int newPhotoCount, string expectedPhotoKeys)
    {
        // Arrange
        var creationFiles = VehicleSalesFixture.SamplePhotoFiles[..creationFileCount];
        var newVehicleSale =
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
        var vehicleSaleId = await _fixture.CreateVehicleSaleAsync(newVehicleSale, creationFiles);

        var existingPhotoKeys = existingPhotosJson is not null
            ? JsonSerializer.Deserialize<string[]>(existingPhotosJson)
            : null;
        // Always use sample3.jpg (jpeg) for new photo uploads during update
        var newPhotoFiles = newPhotoCount > 0
            ? VehicleSalesFixture.SamplePhotoFiles[2..(2 + newPhotoCount)]
            : null;

        await _fixture.UpdateVehicleSale(existingPhotoKeys, newPhotoFiles, vehicleSaleId);

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
