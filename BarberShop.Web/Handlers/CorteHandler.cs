using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Cortes;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Avaliacao;
using BarberShop.Core.Responses.Corte;
using System.Net.Http.Json;

namespace BarberShop.Web.Handlers
{
    public class CorteHandler(IHttpClientFactory httpClientFactory) : ICorteHandler
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

        public async Task<Response<CorteResponse?>> CreateAsync(CreateCorteRequest request)
        {
            var result = await _client.PostAsJsonAsync("v1/cortes", request);
            return await result.Content.ReadFromJsonAsync<Response<CorteResponse?>>()
                ?? new Response<CorteResponse?>(null, 400, "Falha ao criar a corte");
        }

        public async Task<Response<CorteResponse?>> UpdateAsync(UpdateCorteRequest request)
        {
            var result = await _client.PutAsJsonAsync($"v1/cortes/{request.Id}", request);
            return await result.Content.ReadFromJsonAsync<Response<CorteResponse?>>()
                ?? new Response<CorteResponse?>(null, 400, "Falha ao atualizar o corte");
        }

        public async Task<Response<CorteResponse?>> DeleteAsync(long id)
        {
            var result = await _client.DeleteAsync($"v1/cortes/{id}");
            return await result.Content.ReadFromJsonAsync<Response<CorteResponse?>>()
                ?? new Response<CorteResponse?>(null, 400, "Falha ao excluir o corte");
        }

        public async Task<Response<Corte?>> GetByIdAsync(GetCorteByIdRequest request)
            => await _client.GetFromJsonAsync<Response<Corte?>>($"v1/cortes/{request.Id}")
            ?? new Response<Corte?>(null, 400, "Não foi possível obter o corte");


        public async Task<PagedResponse<List<Corte>>> GetAllAsync(GetAllCorteRequest request)
                => await _client.GetFromJsonAsync<PagedResponse<List<Corte>>>(
                    $"v1/cortes?pageNumber={request.PageNumber}&pageSize={request.PageSize}")
                ?? new PagedResponse<List<Corte>>(null, 400, "Não foi possível obter os cortes");
    }
}
