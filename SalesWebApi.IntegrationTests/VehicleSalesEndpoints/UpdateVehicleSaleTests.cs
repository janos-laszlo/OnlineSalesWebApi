using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using ObjectUploadTracking;
using VehicleSales.Dtos;

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
    }

    [Fact]
    public async Task UpdateVehicleSale_WithValidData_ReturnsOkAndUpdatesSale()
    {
        // Arrange
        var vehicleSaleId = await _fixture.CreateVehicleSaleAsync("""
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
        """);
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

    // [Fact]
    // public async Task UpdateVehicleSale_WithInvalidData_ReturnsBadRequest()
    // {
    //     // Arrange
    //     var existingSale = _fixture.GetVehicleSale();
    //     var invalidRequest = new
    //     {
    //         Id = existingSale.Id,
    //         VehicleId = existingSale.VehicleId,
    //         SaleDate = DateTime.UtcNow.AddDays(-1), // Invalid: past date
    //         Price = -100.00m // Invalid: negative price
    //     };

    //     // Act
    //     var response = await _fixture.Client.PutAsJsonAsync(
    //         $"/api/vehicle-sales/{existingSale.Id}",
    //         invalidRequest,
    //         TestContext.Current.CancellationToken);

    //     // Assert
    //     response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    // }

    // [Fact]
    // public async Task UpdateVehicleSale_WithNonExistentId_ReturnsNotFound()
    // {
    //     // Arrange
    //     var nonExistentId = Guid.NewGuid();
    //     var updateRequest = new
    //     {
    //         Id = nonExistentId,
    //         VehicleId = Guid.NewGuid(),
    //         SaleDate = DateTime.UtcNow,
    //         Price = 20000.00m
    //     };

    //     // Act
    //     var response = await _fixture.Client.PutAsJsonAsync(
    //         $"/api/vehicle-sales/{nonExistentId}",
    //         updateRequest,
    //         TestContext.Current.CancellationToken);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    // }

    // [Fact]
    // public async Task UpdateVehicleSale_WithMismatchedId_ReturnsBadRequest()
    // {
    //     // Arrange
    //     var existingSale = _fixture.GetVehicleSale();
    //     var updateRequest = new
    //     {
    //         Id = Guid.NewGuid(), // Mismatched ID
    //         VehicleId = existingSale.VehicleId,
    //         SaleDate = DateTime.UtcNow,
    //         Price = 22000.00m
    //     };

    //     // Act
    //     var response = await _fixture.Client.PutAsJsonAsync(
    //         $"/api/vehicle-sales/{existingSale.Id}",
    //         updateRequest,
    //         TestContext.Current.CancellationToken);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    // }
}
