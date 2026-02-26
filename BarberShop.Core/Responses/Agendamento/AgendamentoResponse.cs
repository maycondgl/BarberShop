namespace BarberShop.Core.Responses.Agendamento;

    public record AgendamentoResponse(
    long Id,
    long ClienteId,
    long CorteId,
    DateTime Data,
    decimal Valor,
    int TempoMinutos,
    string Status
);

