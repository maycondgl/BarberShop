using BarberShop.Core.Enums;

namespace BarberShop.Core.Responses.Agendamentos;

    public record AgendamentoResponse(
    long Id,
    long ClienteId,
    long CorteId,
    DateTime Data,
    decimal Valor,
    int TempoMinutos,
    string Status
);

