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
builder.Services.AddCors(options =>
{
    options.AddPolicy(ApiConfiguration.CorsPolicyName, policy =>
    {
        policy.WithOrigins(
                "https://barbershop-web-gwbhheaaf0cfewgm.centralus-01.azurewebsites.net"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
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
    app.ConfigureDevEnvironment();

app.UseCors(ApiConfiguration.CorsPolicyName);

app.UseAuthentication(); 
app.UseAuthorization();

app.UseSecurity();
app.UseStaticFiles();
app.MapEndpoints();

app.Run();