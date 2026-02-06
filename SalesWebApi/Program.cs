using Microsoft.IdentityModel.Tokens;
using SalesWebApi.Endpoints;
using System.Text;
using Scalar.AspNetCore;
using UserIdentity;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication()
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration.GetValue<string>("Jwt:EncryptionKey")!;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = "http://192.168.0.11:5153",
            ValidateAudience = true,
            ValidAudience = "http://192.168.0.11:5153",
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",   // React
                "http://192.168.0.11:5153/",
                "http://192.168.0.248:3000"    // other client
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddUserIdentity(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("AllowClient");
app.UseUserIdentity();
app.MapIdentityEndpoints();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseDeveloperExceptionPage();
}

app.Run();
