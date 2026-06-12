using BarberShop.Core.Handlers;
using BarberShop.Core.Requests;
using BarberShop.Core.Requests.Account;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Account;
using System.Net.Http.Json;
using System.Text;

namespace BarberShop.Web.Handlers
{
    public class AccountHandler(IHttpClientFactory httpClientFactory) : IAccountHandler
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

        public async Task<Response<string>> LoginAsync(LoginRequest request)
        {
            var result = await _client.PostAsJsonAsync("v1/identity/login?useCookies=true", request);
            return result.IsSuccessStatusCode
                ? new Response<string>("Login realizado com sucesso!", 200, "Login realizado com sucesso!")
                : new Response<string>(null, 400, "Não foi possível fazer login");
        }

        public async Task<Response<string>> RegisterAsync(RegisterRequest request)
        {
            var result = await _client.PostAsJsonAsync("v1/identity/register", request);
            var body = await result.Content.ReadAsStringAsync();

            return result.IsSuccessStatusCode
                ? new Response<string>(body, 201, "Cadastro realizado com sucesso!")
                : new Response<string>(body, (int)result.StatusCode, body);
        }

        public async Task LogoutAsync()
        {
            var emptyContent = new StringContent("{}", Encoding.UTF8, "application/json");
            await _client.PostAsync("v1/identity/logout", emptyContent);
        }

        public async Task<Response<List<AdminUserResponse>>> GetUsersAsync()
            => await _client.GetFromJsonAsync<Response<List<AdminUserResponse>>>("v1/admin/usuarios")
            ?? new Response<List<AdminUserResponse>>(null, 400, "Não foi possível obter os usuários");

        public async Task<Response<string>> AddAdminAsync(long userId)
        {
            var result = await _client.PostAsync($"v1/admin/usuarios/{userId}/admin", null);
            return await result.Content.ReadFromJsonAsync<Response<string>>()
                ?? new Response<string>(null, 400, "Não foi possível promover o usuário");
        }

        public async Task<Response<string>> RemoveAdminAsync(long userId)
        {
            var result = await _client.DeleteAsync($"v1/admin/usuarios/{userId}/admin");
            return await result.Content.ReadFromJsonAsync<Response<string>>()
                ?? new Response<string>(null, 400, "Não foi possível remover o administrador");
        }

        }
    }
