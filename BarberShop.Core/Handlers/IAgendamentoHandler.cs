using BarberShop.Core.Models;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Agendamento;

namespace BarberShop.Core.Handlers
{
    public interface IAgendamentoHandler
    {
        Task<Response<AgendamentoResponse?>> CreateAsync(CreateAgendamentoRequest request);
        Task<Response<AgendamentoResponse?>> UpdateAsync(UpdateAgendamentoRequest request);
        Task<Response<AgendamentoResponse?>> DeleteAsync(long id);
        Task<Response<Agendamento?>> GetByIdAsync(GetAgendamentoByIdRequest request);
        Task<PagedResponse<List<Agendamento>>> GetAllAsync(GetAllAgendamentoRequest request);


    }
}
