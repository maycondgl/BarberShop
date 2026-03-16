using BarberShop.Api.Data;
using BarberShop.Api.Endpoints;
using BarberShop.Api.Handlers;
using BarberShop.Api.Models;
using BarberShop.Core.Handlers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var Connection = builder
    .Configuration
    .GetConnectionString("Connection") ?? string.Empty;

builder
    .Services
    .AddDbContext<BarberShopContext>(
    x =>
    {
        x.UseSqlServer(Connection);
    });
builder.Services
    .AddIdentityCore<User>()
    .AddRoles<IdentityRole<long>>()
    .AddEntityFrameworkStores<BarberShopContext>()
    .AddApiEndpoints();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( x =>
{
    x.CustomSchemaIds(n => n.FullName);
});
builder.Services.AddTransient<IAgendamentoHandler, AgendamentoHandler>();
builder.Services.AddTransient<IAvaliacaoHandler, AvaliacaoHandler>();
builder.Services.AddTransient<IClienteHandler, ClienteHandler>();
builder.Services.AddTransient<ICorteHandler, CorteHandler>();

builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
builder.Services.AddAuthorization();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => new { message = "OK" });
app.MapEndpoints();


app.Run();