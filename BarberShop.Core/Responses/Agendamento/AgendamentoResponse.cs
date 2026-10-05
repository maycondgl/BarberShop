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
    string CorteTitulo = "",
    long? FilialId = null,
    string FilialNome = "",
    long? BarbeiroId = null,
    string BarbeiroNome = ""
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
            !string.IsNullOrWhiteSpace(agendamento.DescricaoServicos) ? agendamento.DescricaoServicos : (agendamento.Corte?.Titulo ?? "Sem corte"),
            agendamento.FilialId,
            agendamento.Filial?.Nome ?? "",
            agendamento.BarbeiroId,
            agendamento.Barbeiro?.Nome ?? ""
        );
}
