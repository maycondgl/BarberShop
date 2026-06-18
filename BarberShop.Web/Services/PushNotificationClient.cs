using System.Net.Http.Json;
using BarberShop.Core.Requests.Notifications;
using BarberShop.Core.Responses.Notifications;
using Microsoft.JSInterop;

namespace BarberShop.Web.Services;

public class PushNotificationClient(
    IHttpClientFactory httpClientFactory,
    IJSRuntime jsRuntime)
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

    public async Task<bool> SubscribeAdminAsync()
    {
        var options = await _client.GetFromJsonAsync<PushNotificationOptionsResponse>("v1/notifications/push-options");

        if (string.IsNullOrWhiteSpace(options?.PublicKey))
            return false;

        var subscription = await jsRuntime.InvokeAsync<PushSubscriptionRequest?>(
            "barberShopNotifications.subscribe",
            options.PublicKey);

        if (subscription is null)
            return false;

        var response = await _client.PostAsJsonAsync("v1/notifications/push-subscriptions", subscription);
        return response.IsSuccessStatusCode;
    }
}
