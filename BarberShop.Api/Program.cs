using BarberShop.Api.Data;
using BarberShop.Api.Handlers;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Requests.Avaliacao;
using BarberShop.Core.Requests.Clientes;
using BarberShop.Core.Requests.Cortes;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Agendamentos;
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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost(
    "/v1/agendamento", async Task<Response<AgendamentoResponse>>
    (CreateAgendamentoRequest request, IAgendamentoHandler handler) 
        => {
            return await handler.CreateAsync(request);
    }
        )
    .WithName("Agendamento: Create")
    .WithSummary("Cadastra um novo serviço de corte")
    .Produces<Agendamento>(201)
    .Produces(403);

app.MapPost("/v1/clientes", async 
    (CreateClienteRequest request, BarberShopContext context) =>
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

app.MapPost("/v1/cortes", async 
    (CreateCorteRequest request, BarberShopContext context) =>
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

app.MapPost("/v1/avaliacoes", async 
    (CreateAvaliacaoRequest request, BarberShopContext context) =>
{

    if (request.Estrelas < 1 || request.Estrelas > 5)
        return Results.BadRequest("A nota deve ser entre 1 e 5 estrelas.");

    var avaliacao = new Avaliacao
    {
        Estrelas = request.Estrelas,
        Comentario = request.Comentario,
        ClienteId = request.ClienteId,
        Data = DateTime.Now
    };

    context.Avaliacoes.Add(avaliacao);
    await context.SaveChangesAsync();

    return Results.Created($"/v1/avaliacoes/{avaliacao.Id}", avaliacao);
});



app.Run();