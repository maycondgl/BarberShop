using BarberShop.Core.Handlers;
using BarberShop.Core.Requests;
using BarberShop.Core.Requests.Account;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Account;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BarberShop.Web.Handlers
{
    public class AccountHandler(IHttpClientFactory httpClientFactory) : IAccountHandler
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

        public async Task<Response<string>> LoginAsync(LoginRequest request)
        {
            try
            {
                var result = await _client.PostAsJsonAsync("v1/identity/login?useCookies=true", request);
                var fallbackMessage = result.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? "E-mail ou senha inválidos."
                    : "Não foi possível fazer login.";

                return await CreateResponseAsync(
                    result,
                    "Login realizado com sucesso!",
                    fallbackMessage);
            }
            catch (HttpRequestException)
            {
                return new Response<string>(
                    null,
                    (int)System.Net.HttpStatusCode.ServiceUnavailable,
                    "Não foi possível conectar ao serviço de autenticação.");
            }
        }

        public async Task<Response<string>> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var result = await _client.PostAsJsonAsync("v1/identity/register", request);
                return await CreateResponseAsync(
                    result,
                    "Cadastro realizado com sucesso!",
                    "Não foi possível realizar o cadastro.");
            }
            catch (HttpRequestException)
            {
                return new Response<string>(
                    null,
                    (int)System.Net.HttpStatusCode.ServiceUnavailable,
                    "Não foi possível conectar ao serviço de cadastro.");
            }
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

        private static async Task<Response<string>> CreateResponseAsync(
            HttpResponseMessage response,
            string successMessage,
            string fallbackMessage)
        {
            var body = await response.Content.ReadAsStringAsync();
            var message = response.IsSuccessStatusCode
                ? successMessage
                : ExtractMessage(body) ?? fallbackMessage;

            return new Response<string>(
                response.IsSuccessStatusCode ? body : null,
                (int)response.StatusCode,
                message);
        }

        private static string? ExtractMessage(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return null;

            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.String)
                    return root.GetString();

                if (root.TryGetProperty("errors", out var errors) &&
                    errors.ValueKind == JsonValueKind.Object)
                {
                    var validationMessages = errors.EnumerateObject()
                        .SelectMany(error => error.Value.ValueKind == JsonValueKind.Array
                            ? error.Value.EnumerateArray()
                                .Where(item => item.ValueKind == JsonValueKind.String)
                                .Select(item => item.GetString())
                            : [])
                        .Where(message => !string.IsNullOrWhiteSpace(message))
                        .Distinct()
                        .ToArray();

                    if (validationMessages.Length > 0)
                        return string.Join(" ", validationMessages!);
                }

                foreach (var propertyName in new[] { "detail", "message", "title" })
                {
                    if (root.TryGetProperty(propertyName, out var property) &&
                        property.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(property.GetString()))
                    {
                        return property.GetString();
                    }
                }
            }
            catch (JsonException)
            {
                return body.Length <= 240 ? body : null;
            }

            return null;
        }

        }
    }
