using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Avaliacao;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Avaliacao;
using System.Net.Http.Json;

namespace BarberShop.Web.Handlers
{
    public class AvaliacaoHandler(IHttpClientFactory httpClientFactory) : IAvaliacaoHandler
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

        public async Task<Response<AvaliacaoResponse?>> CreateAsync(CreateAvaliacaoRequest request)
        {
            var result = await _client.PostAsJsonAsync("v1/avaliacoes", request);
            return await result.Content.ReadFromJsonAsync<Response<AvaliacaoResponse?>>()
                ?? new Response<AvaliacaoResponse?>(null, 400, "Falha ao criar a avaliação");
        }

        public async Task<Response<AvaliacaoResponse?>> UpdateAsync(UpdateAvaliacaoRequest request)
        {
            var result = await _client.PutAsJsonAsync($"v1/avaliacoes/{request.Id}", request);
            return await result.Content.ReadFromJsonAsync<Response<AvaliacaoResponse?>>()
                ?? new Response<AvaliacaoResponse?>(null, 400, "Falha ao atualizar a avaliação");
        }

        public async Task<Response<AvaliacaoResponse?>> DeleteAsync(long id)
        {
            var result = await _client.DeleteAsync($"v1/avaliacoes/{id}");
            return await result.Content.ReadFromJsonAsync<Response<AvaliacaoResponse?>>()
                ?? new Response<AvaliacaoResponse?>(null, 400, "Falha ao excluir a avaliação");
        }
        public async Task<Response<Avaliacao?>> GetByIdAsync(GetAvaliacaoByIdRequest request)
            => await _client.GetFromJsonAsync<Response<Avaliacao?>>($"v1/avaliacoes/{request.Id}")
            ?? new Response<Avaliacao?>(null, 400, "Não foi possível obter a avaliação");

        public async Task<PagedResponse<List<Avaliacao>>> GetAllAsync(GetAllAvaliacaoRequest request)
                => await _client.GetFromJsonAsync<PagedResponse<List<Avaliacao>>>(
                    $"v1/avaliacoes?pageNumber={request.PageNumber}&pageSize={request.PageSize}")
                ?? new PagedResponse<List<Avaliacao>>(null, 400, "Não foi possível obter as avaliações");

    }
}
