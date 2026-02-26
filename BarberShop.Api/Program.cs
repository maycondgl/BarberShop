using BarberShop.Api.Data;
using BarberShop.Api.Endpoints;
using BarberShop.Api.Handlers;
using BarberShop.Core.Handlers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var Connection = builder
    .Configuration
    .GetConnectionString("Connection") ?? string.Empty;

builder.Services.AddDbContext<BarberShopContext>(
    x =>
    {
        x.UseSqlServer(Connection);
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( x =>
{
    x.CustomSchemaIds(n => n.FullName);
});
builder.Services.AddTransient<IAgendamentoHandler, AgendamentoHandler>();
builder.Services.AddTransient<IAvaliacaoHandler, AvaliacaoHandler>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => new { message = "OK" });
app.MapEndpoints();


app.Run();