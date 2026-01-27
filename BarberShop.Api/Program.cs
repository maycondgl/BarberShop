using BarberShop.Api.Data;
using BarberShop.Core.Enums;
using BarberShop.Core.Models;
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
    (Request request, Handler handler) =>
     {
        var result = await handler.Handle(request);
         if (result is null)
             return Results.NotFound("Cliente ou corte não encontrado");


            return Results.Created(
            $"/v1/agendamentos/{result.Id}",
            result
        );

     });

app.MapPost("/v1/clientes", async (ClienteRequest request, BarberShopContext context) =>
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

app.MapPost("/v1/cortes", async (CorteRequest request, BarberShopContext context) =>
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

public class Request
{
        public long ClienteId { get; set; }
        public long CorteId { get; set; }
        public DateTime Data { get; set; }
}

public class AgendamentoResponse
{
    public long Id { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string CorteTitulo { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public TimeSpan Tempo { get; set; }
    public decimal Valor { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class Handler(BarberShopContext context)
{
    public async Task<AgendamentoResponse> Handle(Request request)
    {
        var corte = await context.Cortes.FindAsync(request.CorteId);
        var cliente = await context.Clientes.FindAsync(request.ClienteId);

        if (cliente is null)
            return null;
        if (corte is null)
            throw new Exception("Corte não encontrado");

        var agendamento = new Agendamento()
        {
            ClienteId = cliente.Id,
            CorteId = corte.Id,
            Data = request.Data,
            Tempo = TimeSpan.FromMinutes(corte.DuracaoMinutos),
            ValorPago = corte.Preco,
            Status = EStatusAgendamento.Pendente
        };

        context.Agendamentos.Add(agendamento);
        await context.SaveChangesAsync();

        return new AgendamentoResponse
        {
            Id = agendamento.Id,
            ClienteNome = cliente.Nome,
            CorteTitulo = corte.Titulo,
            Data = agendamento.Data,
            Tempo = agendamento.Tempo,
            Valor = agendamento.ValorPago,
            Status = agendamento.Status.ToString()
        };
    }
}

public class ClienteRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

public class CorteRequest
{
    public string Titulo { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int DuracaoMinutos { get; set; }
    public string Role { get; set; } = string.Empty;
}
