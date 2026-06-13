using BarberShop.Api;
using BarberShop.Api.common.Api;
using BarberShop.Api.Endpoints;
using BarberShop.Api.Models;
using BarberShop.Core;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.AddConfiguration();
builder.AddSecurity();
builder.AddDataContexts();
builder.AddCors();
builder.AddDocumentation();
builder.AddServices();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    Status = "API online",
    Environment = app.Environment.EnvironmentName,
    Date = DateTime.UtcNow
}));

app.MapGet("/config-test", (IConfiguration configuration, IOptions<Secrets> options) => Results.Ok(new
{
    Environment = app.Environment.EnvironmentName,

    HasConnectionString = !string.IsNullOrWhiteSpace(
        configuration.GetConnectionString("Connection")),

    HasJwtTokenSecret = !string.IsNullOrWhiteSpace(
        configuration["Secrets:JwtTokenSecret"]),

    HasApiKey = !string.IsNullOrWhiteSpace(
        configuration["Secrets:ApiKey"]),

    HasPrivateKey = !string.IsNullOrWhiteSpace(
        configuration["Secrets:PrivateKey"]),

    HasBackendUrl = !string.IsNullOrWhiteSpace(
        configuration["BackendUrl"]),

    HasFrontendUrl = !string.IsNullOrWhiteSpace(
        configuration["FrontendUrl"]),

    HasAdminSetupKey = !string.IsNullOrWhiteSpace(
        configuration["AdminSetupKey"]),

    OptionsJwtLoaded = !string.IsNullOrWhiteSpace(
        options.Value.JwtTokenSecret)
}));
if (app.Environment.IsDevelopment())
    app.ConfigureDevEnvironment();

app.UseCors(ApiConfiguration.CorsPolicyName);

app.UseAuthentication(); 
app.UseAuthorization();

app.UseSecurity();
app.UseStaticFiles();
app.MapEndpoints();

app.Run();