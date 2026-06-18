using System.Net.Http.Json;
using BarberShop.Core.Requests.Notifications;
using BarberShop.Core.Responses.Notifications;
using Microsoft.JSInterop;

namespace BarberShop.Web.Services;

public sealed record PushSubscriptionResult(bool Success, string Message);

public class PushNotificationClient(
    IHttpClientFactory httpClientFactory,
    IJSRuntime jsRuntime)
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

    public async Task<PushSubscriptionResult> SubscribeAdminAsync()
    {
        var options = await _client.GetFromJsonAsync<PushNotificationOptionsResponse>("v1/notifications/push-options");

        if (string.IsNullOrWhiteSpace(options?.PublicKey))
            return new(false, "Chaves VAPID não configuradas na API.");

        PushSubscriptionRequest? subscription;

        try
        {
            subscription = await jsRuntime.InvokeAsync<PushSubscriptionRequest?>(
                "barberShopNotifications.subscribe",
                options.PublicKey);
        }
        catch (JSException)
        {
            return new(false, "O navegador bloqueou a inscrição push. Verifique permissões, HTTPS e service worker.");
        }

        if (subscription is null)
            return new(false, "Permissão de notificação negada ou navegador sem suporte a push.");

        var response = await _client.PostAsJsonAsync("v1/notifications/push-subscriptions", subscription);
        return response.IsSuccessStatusCode
            ? new(true, "Notificações ativadas neste dispositivo.")
            : new(false, "A API não conseguiu salvar a inscrição deste dispositivo.");
    }
}
