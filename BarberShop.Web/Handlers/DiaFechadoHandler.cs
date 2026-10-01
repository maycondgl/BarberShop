using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.DiasFechados;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.DiasFechados;
using System.Net.Http.Json;

namespace BarberShop.Web.Handlers
{
    public class DiaFechadoHandler(IHttpClientFactory httpClientFactory) : IDiaFechadoHandler
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

        public async Task<Response<DiaFechadoResponse?>> CreateAsync(CreateDiaFechadoRequest request)
        {
            try
            {
                var result = await _client.PostAsJsonAsync("v1/dias-fechados", request);
                return await result.Content.ReadFromJsonAsync<Response<DiaFechadoResponse?>>()
                    ?? new Response<DiaFechadoResponse?>(null, 400, "Falha ao cadastrar dia fechado");
            }
            catch (Exception ex)
            {
                return new Response<DiaFechadoResponse?>(null, 500, "Erro de comunicação: " + ex.Message);
            }
        }

        public async Task<Response<DiaFechadoResponse?>> DeleteAsync(long id)
        {
            try
            {
                var result = await _client.DeleteAsync($"v1/dias-fechados/{id}");
                return await result.Content.ReadFromJsonAsync<Response<DiaFechadoResponse?>>()
                    ?? new Response<DiaFechadoResponse?>(null, 400, "Falha ao remover dia fechado");
            }
            catch (Exception ex)
            {
                return new Response<DiaFechadoResponse?>(null, 500, "Erro de comunicação: " + ex.Message);
            }
        }

        public async Task<PagedResponse<List<DiaFechado>>> GetAllAsync(GetAllDiasFechadosRequest request)
        {
            try
            {
                var response = await _client.GetAsync($"v1/dias-fechados?pageNumber={request.PageNumber}&pageSize={request.PageSize}");
                if (!response.IsSuccessStatusCode)
                    return new PagedResponse<List<DiaFechado>>(new List<DiaFechado>(), 0, request.PageNumber, request.PageSize);

                return await response.Content.ReadFromJsonAsync<PagedResponse<List<DiaFechado>>>()
                    ?? new PagedResponse<List<DiaFechado>>(new List<DiaFechado>(), 0, request.PageNumber, request.PageSize);
            }
            catch
            {
                // Retorno seguro de lista vazia para não interromper a tela caso a rota ou tabela não responda
                return new PagedResponse<List<DiaFechado>>(new List<DiaFechado>(), 0, request.PageNumber, request.PageSize);
            }
        }
    }
}
