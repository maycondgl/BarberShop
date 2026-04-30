using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests;
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
        { 
            var response = await _client.GetAsync($"v1/avaliacoes/{request.Id}");

            if (!response.IsSuccessStatusCode)
                return new Response<Avaliacao?>(null, (int) response.StatusCode, "Não foi possível obter a avaliação");

            return await response.Content.ReadFromJsonAsync<Response<Avaliacao?>>()
                ?? new Response<Avaliacao?>(null, 400, "Não foi possível obter a avaliação");
        }

        public async Task<PagedResponse<List<AvaliacaoResponse>>> GetAllAsync(GetAllAvaliacaoRequest request)
        {
            var response = await _client.GetAsync(
                $"v1/avaliacoes?pageNumber={request.PageNumber}&pageSize={request.PageSize}");

            if (!response.IsSuccessStatusCode)
                return new PagedResponse<List<AvaliacaoResponse>>(null, (int)response.StatusCode, "Não foi possível obter as avaliações");

            return await response.Content.ReadFromJsonAsync<PagedResponse<List<AvaliacaoResponse>>>()
                ?? new PagedResponse<List<AvaliacaoResponse>>(null, 400, "Não foi possível obter as avaliações");
        }
        public async Task<PagedResponse<List<AvaliacaoResponse>>> GetAllPublicAsync(int pageNumber, int pageSize)
        {
            var response = await _client.GetAsync(
                $"v1/public/avaliacoes?pageNumber={pageNumber}&pageSize={pageSize}");

            if (!response.IsSuccessStatusCode)
                return new PagedResponse<List<AvaliacaoResponse>>(null, (int)response.StatusCode, "Não foi possível obter as avaliações");

            return await response.Content.ReadFromJsonAsync<PagedResponse<List<AvaliacaoResponse>>>()
                ?? new PagedResponse<List<AvaliacaoResponse>>(null, 400, "Não foi possível obter as avaliações");
        }
    }
}
