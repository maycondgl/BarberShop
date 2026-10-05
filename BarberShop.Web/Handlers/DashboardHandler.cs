using BarberShop.Core.Handlers;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Dashboard;
using System.Net.Http.Json;

namespace BarberShop.Web.Handlers;

public class DashboardHandler(IHttpClientFactory httpClientFactory) : IDashboardHandler
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

    public async Task<Response<DashboardResponse?>> GetDashboardAsync(long? filialId = null)
    {
        try
        {
            var url = filialId.HasValue && filialId.Value > 0
                ? $"v1/admin/dashboard?filialId={filialId.Value}"
                : "v1/admin/dashboard";

            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<Response<DashboardResponse?>>();
                return errorResponse ?? new Response<DashboardResponse?>(null, (int)response.StatusCode, "Falha ao carregar dados do dashboard");
            }

            return await response.Content.ReadFromJsonAsync<Response<DashboardResponse?>>()
                ?? new Response<DashboardResponse?>(null, 400, "Dados do dashboard indisponíveis");
        }
        catch (Exception ex)
        {
            return new Response<DashboardResponse?>(null, 500, "Erro de comunicação ao carregar dashboard: " + ex.Message);
        }
    }
}
