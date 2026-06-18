using BarberShop.Core.Responses.Agendamento;

namespace BarberShop.Api.Services;

public interface IAgendamentoNotificationService
{
    Task NotifyNovoAgendamentoAsync(AgendamentoResponse agendamento, CancellationToken cancellationToken = default);
}
