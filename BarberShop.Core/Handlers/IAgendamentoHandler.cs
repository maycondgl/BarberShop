using BarberShop.Core.Models;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Agendamentos;

namespace BarberShop.Core.Handlers
{
    public interface IAgendamentoHandler
    {
        Task<Response<AgendamentoResponse>> CreateAsync(CreateAgendamentoRequest request);
        Task<Response<Agendamento>> UpdateAsync(UpdateAgendamentoRequest request);
        Task<Response<Agendamento>> DeleteAsync(DeleteAgendamentoRequest request);
        Task<Response<Agendamento>> GetByIdAsync(GetAgendamentoByIdRequest request);
        Task<Response<List<Agendamento>>> GetAllAsync(GetAllAgendamentoRequest request);


    }
}
