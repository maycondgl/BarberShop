using System.Net.Http.Json;
using BarberShop.Core.Requests.Notifications;
using BarberShop.Core.Responses.Notifications;
using Microsoft.JSInterop;

namespace BarberShop.Web.Services;

public sealed record PushSubscriptionResult(bool Success, string Message);

public sealed record BrowserPushSubscriptionResult(
    bool Success,
    string Message,
    PushSubscriptionRequest? Subscription);

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

        BrowserPushSubscriptionResult? browserResult;

        try
        {
            browserResult = await jsRuntime.InvokeAsync<BrowserPushSubscriptionResult?>(
                "barberShopNotifications.subscribe",
                options.PublicKey);
        }
        catch (JSException ex)
        {
            return new(false, $"O navegador não pôde completar a inscrição push: {ex.Message}");
        }

        if (browserResult is null)
            return new(false, "Não foi possível verificar suporte a push neste navegador.");

        if (!browserResult.Success || browserResult.Subscription is null)
            return new(false, browserResult.Message);

        var response = await _client.PostAsJsonAsync("v1/notifications/push-subscriptions", browserResult.Subscription);
        return response.IsSuccessStatusCode
            ? new(true, browserResult.Message)
            : new(false, "A API não conseguiu salvar a inscrição deste dispositivo.");
    }

    public async Task<NotificationPermissionStatus> GetPermissionStateAsync()
    {
        try
        {
            return await jsRuntime.InvokeAsync<NotificationPermissionStatus>(
                "barberShopNotifications.getPermissionState");
        }
        catch
        {
            return new(false, "unsupported", "Erro ao verificar suporte a notificações.");
        }
    }

    public async Task<bool> IsPromptDismissedAsync(string storageKey = "barbershop_admin_push_dismissed")
    {
        try
        {
            return await jsRuntime.InvokeAsync<bool>(
                "barberShopNotifications.isDismissed", storageKey);
        }
        catch
        {
            return false;
        }
    }

    public async Task SetPromptDismissedAsync(string storageKey = "barbershop_admin_push_dismissed", int durationDays = 7)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync(
                "barberShopNotifications.setDismissed", storageKey, durationDays);
        }
        catch
        {
        }
    }

    public async Task<bool> IsSubscribedAsync()
    {
        try
        {
            return await jsRuntime.InvokeAsync<bool>(
                "barberShopNotifications.isSubscribed");
        }
        catch
        {
            return false;
        }
    }
}

public sealed record NotificationPermissionStatus(bool Supported, string Permission, string Message);
