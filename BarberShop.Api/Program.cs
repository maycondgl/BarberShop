using BarberShop.Api;
using BarberShop.Api.common.Api;
using BarberShop.Api.Endpoints;
using BarberShop.Api.Models;
using BarberShop.Core;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.AddConfiguration();
builder.AddDataContexts();
builder.AddSecurity();
builder.AddCors();
builder.AddDocumentation();
builder.AddServices();

var app = builder.Build();

app.UseCors(ApiConfiguration.CorsPolicyName);

app.MapGet("/health", () => Results.Ok(new
{
    Status = "API online",
    Environment = app.Environment.EnvironmentName,
    Date = DateTime.UtcNow
}));

if (app.Environment.IsDevelopment())
    app.ConfigureDevEnvironment();



app.UseAuthentication(); 
app.UseAuthorization();

app.UseSecurity();
app.UseStaticFiles();
app.MapEndpoints();

app.Run();