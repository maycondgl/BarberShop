using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Agendamento;
using System.Net.Http.Json;

namespace BarberShop.Web.Handlers
{
    public class AgendamentoHandler(IHttpClientFactory httpClientFactory) : IAgendamentoHandler
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

        public async Task<Response<AgendamentoResponse?>> CreateAsync(CreateAgendamentoRequest request)
        {
            var result = await _client.PostAsJsonAsync("v1/agendamentos", request);
            return await result.Content.ReadFromJsonAsync<Response<AgendamentoResponse?>>()
                ?? new Response<AgendamentoResponse?>(null, 400, "Falha ao criar o agendamento");
        }

        public async Task<Response<AgendamentoResponse?>> UpdateAsync(UpdateAgendamentoRequest request)
        {
            var result = await _client.PutAsJsonAsync($"v1/agendamentos/{request.Id}", request);
            return await result.Content.ReadFromJsonAsync<Response<AgendamentoResponse?>>()
                ?? new Response<AgendamentoResponse?>(null, 400, "Falha ao atualizar o agendamento");
        }

        public async Task<Response<AgendamentoResponse?>> DeleteAsync(long id)
        {
            var result = await _client.DeleteAsync($"v1/agendamentos/{id}");
            return await result.Content.ReadFromJsonAsync<Response<AgendamentoResponse?>>()
                ?? new Response<AgendamentoResponse?>(null, 400, "Falha ao excluir o agendamento");
        }

        public async Task<Response<Agendamento?>> GetByIdAsync(GetAgendamentoByIdRequest request)
            => await _client.GetFromJsonAsync<Response<Agendamento?>>($"v1/agendamentos/{request.Id}")
            ?? new Response<Agendamento?>(null, 400, "Não foi possível obter agendamento");

        public async Task<PagedResponse<List<Agendamento>>> GetAllAsync(GetAllAgendamentoRequest request)
     => await _client.GetFromJsonAsync<PagedResponse<List<Agendamento>>>(
         $"v1/agendamentos?pageNumber={request.PageNumber}&pageSize={request.PageSize}")
         ?? new PagedResponse<List<Agendamento>>(null, 400, "Não foi possível obter os agendamentos");

        public async Task<PagedResponse<List<AgendamentoResponse>?>> GetByPeriodAsync(
                    GetAgendamentoByPeriodRequest request)
        {
            return await _client
                .GetFromJsonAsync<PagedResponse<List<AgendamentoResponse>?>>(
                    $"v1/agendamentos/period?startDate={request.StartDate:yyyy-MM-dd}" +
                    $"&endDate={request.EndDate:yyyy-MM-dd}" +
                    $"&pageNumber={request.PageNumber}" +
                    $"&pageSize={request.PageSize}")
                ?? new PagedResponse<List<AgendamentoResponse>?>(
                    null, 400, "Não foi possível obter os agendamentos");
        }
    }
}
