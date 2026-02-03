using BarberShop.Api.Data;
using BarberShop.Core.Enums;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Clientes;
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
builder.Services.AddTransient<Handler>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost(
    "/v1/agendamento", async
    (CreateAgendamentoRequest request, AgendamentoHandler handler) =>
     {
        var result = await handler.Handle(request);
         if (result is null)
             return Results.NotFound("Cliente ou corte não encontrado");


            return Results.Created(
            $"/v1/agendamentos/{result.Id}",
            result
        );

     });

app.MapPost("/v1/clientes", async (CreateClienteRequest request, BarberShopContext context) =>
{
    var cliente = new Cliente
    {
        Nome = request.Nome,
        Telefone = request.Telefone,
        UserId = request.UserId
    };

    context.Clientes.Add(cliente);
    await context.SaveChangesAsync();

    return Results.Created($"/v1/clientes/{cliente.Id}", cliente);
});

app.MapPost("/v1/cortes", async (CreateCorteRequest request, BarberShopContext context) =>
{
    if (request.Role != "Admin")
        return Results.StatusCode(403);


    var corte = new Corte
    {
        Titulo = request.Titulo,
        Preco = request.Preco,
        DuracaoMinutos = request.DuracaoMinutos
    };

    context.Cortes.Add(corte);
    await context.SaveChangesAsync();

    return Results.Created($"/v1/cortes/{corte.Id}", corte);
});



app.Run();