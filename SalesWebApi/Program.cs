using Microsoft.AspNetCore.Identity;
using SalesWebApi.Endpoints;
using UserIdentity;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddUserIdentity(builder.Configuration);

var app = builder.Build();

app.MapIdentityEndpoints();

app.Run();
