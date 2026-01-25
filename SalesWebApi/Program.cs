using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SalesWebApi.Endpoints;
using System.Text;
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
            ValidIssuer = "http://localhost:5152",
            ValidateAudience = true,
            ValidAudience = "http://localhost:5152",
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddUserIdentity(builder.Configuration);

var app = builder.Build();

app.UseUserIdentity();
app.MapIdentityEndpoints();

app.Run();
