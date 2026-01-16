var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

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
    public bool IsCompleted { get; set; }
}

public class Handler { 
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
