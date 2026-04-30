namespace BarberShop.Core.Responses.Agendamento;

public record AgendamentoResponse(
    long Id,
    long UserId,
    long CorteId,
    DateTime Data,
    decimal Valor,
    int TempoMinutos,
    string Status,
    string NomeCliente,
    string CorteTitulo = ""
)
{
    public static implicit operator AgendamentoResponse(Models.Agendamento agendamento)
        => new(
            agendamento.Id,
            agendamento.UserId,
            agendamento.CorteId,
            agendamento.Data,
            agendamento.Valor,
            (int)agendamento.Tempo.TotalMinutes,
            agendamento.Status.ToString(),
            agendamento.NomeCliente,
            agendamento.Corte?.Titulo ?? "Sem corte"
        );
}

