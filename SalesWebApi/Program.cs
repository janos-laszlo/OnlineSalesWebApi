using System.Text;
using Common;
using DraftEntities;
using EmailSending;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using SalesWebApi.Endpoints;
using Scalar.AspNetCore;
using UserIdentity;
using VehicleSales;

const string CorsPolicyName = "AllowClient";

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication()
    .AddJwtBearer(ConfigureJwtBearer(builder));
builder.Services
    .AddAuthorization()
    .AddCors(ConfigureCors(builder))
    .AddOpenApi()
    .AddProblemDetails()
    .AddValidation()
    .AddEmailSending(builder.Configuration)
    .AddUserIdentity(builder.Configuration)
    .AddVehicleSales(builder.Configuration)
    .AddDraftEntities(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler()
    .UseCors(CorsPolicyName);
app.UseUserIdentity();
app.MapIdentityEndpoints();
app.MapVehicleSalesEndpoints();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseDeveloperExceptionPage();
}

app.Run();

static Action<JwtBearerOptions> ConfigureJwtBearer(WebApplicationBuilder builder) =>
    options =>
    {
        var jwtKey = builder.Configuration.GetValue<string>(Constants.ConfigKeys.JwtEncryptionKey)!;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = "http://192.168.1.8:5153",
            ValidateAudience = true,
            ValidAudience = "http://192.168.1.8:5153",
            ValidateLifetime = true
        };
    };

static Action<CorsOptions> ConfigureCors(WebApplicationBuilder builder) =>
    options =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]?>();
        if (!(allowedOrigins?.Length > 0))
            throw new InvalidOperationException("AllowedOrigins configuration is missing or empty.");
        options.AddPolicy(CorsPolicyName, policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
    };
