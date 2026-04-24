using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using UserIdentity.Commands;
using VehicleSales;
using VehicleSales.Queries;

namespace SalesWebApi.IntegrationTests;

public sealed class VehicleSalesFixture : IDisposable
{
    internal const string CollectionName = "Vehicle Sales";
    internal static readonly string PhotosDirectory = Path.Combine(AppContext.BaseDirectory, "VehicleSalesEndpoints", "Data");
    internal static readonly string[] SamplePhotoFiles =
    [
        Path.Combine(PhotosDirectory, "sample1.jpg"),
        Path.Combine(PhotosDirectory, "sample2.png"),
        Path.Combine(PhotosDirectory, "sample3.jpg"),
    ];

    private readonly WebApplicationFactory<Program> app;
    private readonly IServiceScope scope;
    private readonly IConfiguration configuration;
    private readonly IAmazonS3 s3Client;
    internal HttpClient Client { get; }
    internal string AccessToken { get; }
    public Lazy<int> AnotherUsersVehicleSaleId { get; }

    public VehicleSalesFixture()
    {
        app = new WebApplicationFactory<Program>();
        scope = app.Services.CreateScope();
        configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        s3Client = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        Client = app.CreateClient();
        AccessToken = RegisterAndLoginUser(new UserCredentialsDto(UserUtils.NextEmail, "Password1")).AccessToken;
        AnotherUsersVehicleSaleId = new Lazy<int>(CreateVehicleSaleForAnotherUser());
    }

    private Func<int> CreateVehicleSaleForAnotherUser()
    {
        return () =>
        {
            var otherUserToken = RegisterAndLoginUser(new UserCredentialsDto(UserUtils.NextEmail, "Password1")).AccessToken;
            var form = BuildFormFromJson(
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
                """);
            var httpRequest = new HttpRequestMessage(
                HttpMethod.Post, Endpoints.VehicleSalesEndpoints.VehicleSalesBase)
            {
                Content = form
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", otherUserToken);
            var response = Client.SendAsync(httpRequest, TestContext.Current.CancellationToken).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var location = response.Headers.Location?.OriginalString;
            Assert.NotNull(location);
            var idString = location.Split('/').Last();
            return int.Parse(idString);
        };
    }

    public void Dispose()
    {
        Client.Dispose();
        scope.Dispose();
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

    internal Task<VehicleSaleFullDto?> GetVehicleSaleAsync(int id) =>
        Client.GetFromJsonAsync<VehicleSaleFullDto>($"{VehicleSalesUris.GetVehicleSaleById}{id}");

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

    internal async Task<int> CreateVehicleSaleAsync(string requestBody, string[]? photoFiles = null)
    {
        var httpRequest = CreateVehicleSaleRequest(requestBody, photoFiles);
        var response = await Client.SendAsync(httpRequest, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location?.OriginalString;
        Assert.NotNull(location);
        return int.Parse(location.Split('/').Last());
    }

    internal async Task UpdateVehicleSale(string[]? existingPhotoKeys, string[]? newPhotoFiles, int vehicleSaleId)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent("San Francisco"), "county");
        form.Add(new StringContent("Santa Monica1"), "locality");
        form.Add(new StringContent("80"), "vehicleModelId");

        if (existingPhotoKeys is not null)
        {
            foreach (var key in existingPhotoKeys)
                form.Add(new StringContent(key), "existingPhotos");
        }

        AddPhotoFilesToForm(form, "photos", newPhotoFiles);

        var updateRequest = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{Endpoints.VehicleSalesEndpoints.VehicleSalesBase}/{vehicleSaleId}")
        {
            Content = form
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", AccessToken);
        var response = await Client.SendAsync(
            updateRequest,
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    internal HttpRequestMessage CreateVehicleSaleRequest(string requestBodyJson, string[]? photoFiles = null)
    {
        var form = BuildFormFromJson(requestBodyJson);
        AddPhotoFilesToForm(form, "photos", photoFiles);
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post, Endpoints.VehicleSalesEndpoints.VehicleSalesBase)
        {
            Content = form
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", AccessToken);
        return httpRequest;
    }

    internal HttpRequestMessage BuildUpdateRequest(int vehicleSaleId, string requestBodyJson, string[]? existingPhotoKeys = null, string[]? newPhotoFiles = null)
    {
        var form = BuildFormFromJson(requestBodyJson);

        if (existingPhotoKeys is not null)
        {
            foreach (var key in existingPhotoKeys)
                form.Add(new StringContent(key), "existingPhotos");
        }

        AddPhotoFilesToForm(form, "photos", newPhotoFiles);

        var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{Endpoints.VehicleSalesEndpoints.VehicleSalesBase}/{vehicleSaleId}")
        {
            Content = form
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
        return request;
    }

    internal async Task<string[]> GetObjectsInDirectory(
        string directory,
        CancellationToken cancellation)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = configuration[R2Config.BucketNameKey],
            Prefix = directory
        };

        var response = await s3Client.ListObjectsV2Async(request, cancellation);

        return response.S3Objects.Select(o => o.Key[(directory.Length + 1)..]).ToArray();
    }

    private static MultipartFormDataContent BuildFormFromJson(string json)
    {
        var form = new MultipartFormDataContent();
        using var jsonDoc = JsonDocument.Parse(json);
        foreach (var property in jsonDoc.RootElement.EnumerateObject())
        {
            // Skip old API fields
            if (property.Name.Equals("photoContentTypes", StringComparison.OrdinalIgnoreCase))
                continue;

            if (property.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in property.Value.EnumerateArray())
                {
                    var val = item.ValueKind == JsonValueKind.String
                        ? item.GetString()!
                        : item.GetRawText();
                    form.Add(new StringContent(val), property.Name);
                }
            }
            else if (property.Value.ValueKind != JsonValueKind.Null)
            {
                var val = property.Value.ValueKind == JsonValueKind.String
                    ? property.Value.GetString()!
                    : property.Value.GetRawText();
                form.Add(new StringContent(val), property.Name);
            }
        }
        return form;
    }

    private static void AddPhotoFilesToForm(MultipartFormDataContent form, string fieldName, string[]? photoFiles)
    {
        if (photoFiles is null) return;
        foreach (var filePath in photoFiles)
        {
            var fileName = Path.GetFileName(filePath);
            var contentType = GetContentTypeFromFileExtension(fileName);
            var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            form.Add(fileContent, fieldName, fileName);
        }
    }

    private static string GetContentTypeFromFileExtension(string filename)
    {
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => throw new NotSupportedException($"File extension {extension} is not supported.")
        };
    }
}

[CollectionDefinition(VehicleSalesFixture.CollectionName)]
public class VehicleSalesCollection : ICollectionFixture<VehicleSalesFixture>
{ }
