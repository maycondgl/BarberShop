using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Filiais;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Filial;
using System.Net.Http.Json;

namespace BarberShop.Web.Handlers
{
    public class FilialHandler(IHttpClientFactory httpClientFactory) : IFilialHandler
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

        public async Task<Response<FilialResponse?>> CreateAsync(CreateFilialRequest request)
        {
            try
            {
                var result = await _client.PostAsJsonAsync("v1/filiais", request);
                return await result.Content.ReadFromJsonAsync<Response<FilialResponse?>>()
                    ?? new Response<FilialResponse?>(null, 400, "Falha ao cadastrar a filial");
            }
            catch (Exception ex)
            {
                return new Response<FilialResponse?>(null, 500, "Erro de comunicação: " + ex.Message);
            }
        }

        public async Task<Response<FilialResponse?>> UpdateAsync(UpdateFilialRequest request)
        {
            try
            {
                var result = await _client.PutAsJsonAsync($"v1/filiais/{request.Id}", request);
                return await result.Content.ReadFromJsonAsync<Response<FilialResponse?>>()
                    ?? new Response<FilialResponse?>(null, 400, "Falha ao atualizar a filial");
            }
            catch (Exception ex)
            {
                return new Response<FilialResponse?>(null, 500, "Erro de comunicação: " + ex.Message);
            }
        }

        public async Task<Response<FilialResponse?>> DeleteAsync(long id)
        {
            try
            {
                var result = await _client.DeleteAsync($"v1/filiais/{id}");
                return await result.Content.ReadFromJsonAsync<Response<FilialResponse?>>()
                    ?? new Response<FilialResponse?>(null, 400, "Falha ao excluir a filial");
            }
            catch (Exception ex)
            {
                return new Response<FilialResponse?>(null, 500, "Erro de comunicação: " + ex.Message);
            }
        }

        public async Task<Response<Filial?>> GetByIdAsync(GetFilialByIdRequest request)
        {
            try
            {
                return await _client.GetFromJsonAsync<Response<Filial?>>($"v1/filiais/{request.Id}")
                    ?? new Response<Filial?>(null, 404, "Filial não encontrada");
            }
            catch (Exception ex)
            {
                return new Response<Filial?>(null, 500, "Erro de comunicação: " + ex.Message);
            }
        }

        public async Task<PagedResponse<List<Filial>>> GetAllAsync(GetAllFilialRequest request)
        {
            try
            {
                var url = $"v1/filiais?pageNumber={request.PageNumber}&pageSize={request.PageSize}";
                if (request.ApenasAtivos.HasValue)
                {
                    url += $"&apenasAtivos={request.ApenasAtivos.Value.ToString().ToLowerInvariant()}";
                }

                var response = await _client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return new PagedResponse<List<Filial>>(new List<Filial>(), 0, request.PageNumber, request.PageSize);

                return await response.Content.ReadFromJsonAsync<PagedResponse<List<Filial>>>()
                    ?? new PagedResponse<List<Filial>>(new List<Filial>(), 0, request.PageNumber, request.PageSize);
            }
            catch
            {
                return new PagedResponse<List<Filial>>(new List<Filial>(), 0, request.PageNumber, request.PageSize);
            }
        }
    }
}
