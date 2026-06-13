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

if (app.Environment.IsDevelopment())
{
    app.MapGet("/config-test", (IOptions<Secrets> options) => new
    {
        HasJwtTokenSecret = !string.IsNullOrWhiteSpace(options.Value.JwtTokenSecret),
        HasApiKey = !string.IsNullOrWhiteSpace(options.Value.ApiKey),
        HasPrivateKey = !string.IsNullOrWhiteSpace(options.Value.PrivateKey),
        HasConnectionString = !string.IsNullOrWhiteSpace(Configuration.Connection)
    });
}
if (app.Environment.IsDevelopment())
    app.ConfigureDevEnvironment();

app.UseCors(ApiConfiguration.CorsPolicyName);

app.UseAuthentication(); 
app.UseAuthorization();

app.UseSecurity();
app.UseStaticFiles();
app.MapEndpoints();

app.Run();