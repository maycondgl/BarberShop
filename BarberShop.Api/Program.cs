using BarberShop.Api.Data;
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
    "/v1/agendamento", 
    (Request request, Handler handler)  
        => handler.Handle(request))
    .WithName("Agendamento: Create")
    .WithSummary("Cria um novo agendamento")
    .Produces<AgendamentoResponse>();

app.Run();

public class Request
{
    public long ClienteId { get; set; }
    public long CorteId { get; set; }
    public DateTime Data { get; set; }
    public string Tempo { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

public class AgendamentoResponse
{
    public long Id { get; set; }
    public string ClienteNome { get; set; } = string.Empty; 
    public string CorteTitulo { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string Tempo { get; set; } = string.Empty;
    public int Status { get; set; } = 1;
}

public class Handler
{
    public AgendamentoResponse Handle(Request request)
    {
        return new AgendamentoResponse
        {
            Id = 4,
            ClienteNome = $"Cliente ID: {request.ClienteId}",
            CorteTitulo = $"Corte ID: {request.CorteId}",
            Data = request.Data,
            Tempo = request.Tempo
        };
    }
}