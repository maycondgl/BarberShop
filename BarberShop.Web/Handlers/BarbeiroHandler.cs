using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Barbeiros;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Barbeiro;
using System.Net.Http.Json;

namespace BarberShop.Web.Handlers
{
    public class BarbeiroHandler(IHttpClientFactory httpClientFactory) : IBarbeiroHandler
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

        public async Task<Response<BarbeiroResponse?>> CreateAsync(CreateBarbeiroRequest request)
        {
            try
            {
                var result = await _client.PostAsJsonAsync("v1/barbeiros", request);
                return await result.Content.ReadFromJsonAsync<Response<BarbeiroResponse?>>()
                    ?? new Response<BarbeiroResponse?>(null, 400, "Falha ao cadastrar o barbeiro");
            }
            catch (Exception ex)
            {
                return new Response<BarbeiroResponse?>(null, 500, "Erro de comunicação: " + ex.Message);
            }
        }

        public async Task<Response<BarbeiroResponse?>> UpdateAsync(UpdateBarbeiroRequest request)
        {
            try
            {
                var result = await _client.PutAsJsonAsync($"v1/barbeiros/{request.Id}", request);
                return await result.Content.ReadFromJsonAsync<Response<BarbeiroResponse?>>()
                    ?? new Response<BarbeiroResponse?>(null, 400, "Falha ao atualizar o barbeiro");
            }
            catch (Exception ex)
            {
                return new Response<BarbeiroResponse?>(null, 500, "Erro de comunicação: " + ex.Message);
            }
        }

        public async Task<Response<BarbeiroResponse?>> DeleteAsync(long id)
        {
            try
            {
                var result = await _client.DeleteAsync($"v1/barbeiros/{id}");
                return await result.Content.ReadFromJsonAsync<Response<BarbeiroResponse?>>()
                    ?? new Response<BarbeiroResponse?>(null, 400, "Falha ao excluir o barbeiro");
            }
            catch (Exception ex)
            {
                return new Response<BarbeiroResponse?>(null, 500, "Erro de comunicação: " + ex.Message);
            }
        }

        public async Task<Response<Barbeiro?>> GetByIdAsync(GetBarbeiroByIdRequest request)
        {
            try
            {
                return await _client.GetFromJsonAsync<Response<Barbeiro?>>($"v1/barbeiros/{request.Id}")
                    ?? new Response<Barbeiro?>(null, 404, "Barbeiro não encontrado");
            }
            catch (Exception ex)
            {
                return new Response<Barbeiro?>(null, 500, "Erro de comunicação: " + ex.Message);
            }
        }

        public async Task<PagedResponse<List<Barbeiro>>> GetAllAsync(GetAllBarbeiroRequest request)
        {
            try
            {
                var url = $"v1/barbeiros?pageNumber={request.PageNumber}&pageSize={request.PageSize}";
                if (request.FilialId.HasValue && request.FilialId.Value > 0)
                {
                    url += $"&filialId={request.FilialId.Value}";
                }
                if (request.ApenasAtivos.HasValue)
                {
                    url += $"&apenasAtivos={request.ApenasAtivos.Value.ToString().ToLowerInvariant()}";
                }

                var response = await _client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return new PagedResponse<List<Barbeiro>>(new List<Barbeiro>(), 0, request.PageNumber, request.PageSize);

                return await response.Content.ReadFromJsonAsync<PagedResponse<List<Barbeiro>>>()
                    ?? new PagedResponse<List<Barbeiro>>(new List<Barbeiro>(), 0, request.PageNumber, request.PageSize);
            }
            catch
            {
                return new PagedResponse<List<Barbeiro>>(new List<Barbeiro>(), 0, request.PageNumber, request.PageSize);
            }
        }

        public async Task<Response<Barbeiro?>> GetByUserIdAsync(long userId)
        {
            try
            {
                var response = await _client.GetAsync($"v1/barbeiros/usuario/{userId}");
                if (!response.IsSuccessStatusCode)
                    return new Response<Barbeiro?>(null, (int)response.StatusCode, "Barbeiro não encontrado para o usuário");

                return await response.Content.ReadFromJsonAsync<Response<Barbeiro?>>()
                    ?? new Response<Barbeiro?>(null, 404, "Barbeiro não encontrado");
            }
            catch (Exception ex)
            {
                return new Response<Barbeiro?>(null, 500, "Erro de comunicação: " + ex.Message);
            }
        }

        public async Task<Response<BarbeiroDashboardResponse?>> GetDashboardAsync(long? barbeiroId = null)
        {
            try
            {
                var url = barbeiroId.HasValue && barbeiroId.Value > 0
                    ? $"v1/barbeiros/dashboard?barbeiroId={barbeiroId.Value}"
                    : "v1/barbeiros/dashboard";

                var response = await _client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<Response<BarbeiroDashboardResponse?>>();
                    return errorResult ?? new Response<BarbeiroDashboardResponse?>(null, (int)response.StatusCode, "Falha ao carregar dashboard do barbeiro");
                }

                return await response.Content.ReadFromJsonAsync<Response<BarbeiroDashboardResponse?>>()
                    ?? new Response<BarbeiroDashboardResponse?>(null, 404, "Dados do dashboard não encontrados");
            }
            catch (Exception ex)
            {
                return new Response<BarbeiroDashboardResponse?>(null, 500, "Erro de comunicação: " + ex.Message);
            }
        }

        public async Task<Response<BarbeiroUsuarioInfoResponse?>> BuscarUsuarioPorEmailAsync(string email)
        {
            try
            {
                var encoded = Uri.EscapeDataString(email.Trim());
                var response = await _client.GetAsync($"v1/barbeiros/buscar-usuario?email={encoded}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<Response<BarbeiroUsuarioInfoResponse?>>();
                    return errorResult ?? new Response<BarbeiroUsuarioInfoResponse?>(null, (int)response.StatusCode, "Usuário não encontrado.");
                }

                return await response.Content.ReadFromJsonAsync<Response<BarbeiroUsuarioInfoResponse?>>()
                    ?? new Response<BarbeiroUsuarioInfoResponse?>(null, 404, "Usuário não encontrado.");
            }
            catch (Exception ex)
            {
                return new Response<BarbeiroUsuarioInfoResponse?>(null, 500, "Erro ao buscar usuário: " + ex.Message);
            }
        }
    }
}
