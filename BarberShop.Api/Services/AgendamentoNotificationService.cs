using System.Net;
using System.Text.Json;
using BarberShop.Api.Data;
using BarberShop.Api.Hubs;
using BarberShop.Api.Models;
using BarberShop.Core.Responses.Agendamento;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebPush;

namespace BarberShop.Api.Services;

public class AgendamentoNotificationService(
    IHubContext<AgendamentoHub> hubContext,
    BarberShopContext context,
    IOptions<Secrets> secretsOptions,
    ILogger<AgendamentoNotificationService> logger) : IAgendamentoNotificationService
{
    private readonly Secrets _secrets = secretsOptions.Value;

    public async Task NotifyNovoAgendamentoAsync(
        AgendamentoResponse agendamento,
        CancellationToken cancellationToken = default)
    {
        await hubContext.Clients
            .Group(AgendamentoHub.AdminGroupName)
            .SendAsync("NovoAgendamento", agendamento, cancellationToken);

        await NotifyPushAsync(agendamento, cancellationToken);
    }

    private async Task NotifyPushAsync(
        AgendamentoResponse agendamento,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_secrets.VapidPublicKey) ||
            string.IsNullOrWhiteSpace(_secrets.VapidPrivateKey) ||
            string.IsNullOrWhiteSpace(_secrets.VapidSubject))
        {
            logger.LogInformation("Web Push nao configurado. Pulando notificacao push.");
            return;
        }

        var subscriptions = await context.PushSubscriptionDevices
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (subscriptions.Count == 0)
            return;

        var payload = JsonSerializer.Serialize(new
        {
            title = "Novo agendamento",
            body = $"{agendamento.NomeCliente} marcou {agendamento.CorteTitulo} para {agendamento.Data:dd/MM/yyyy HH:mm}.",
            url = "/admin/agendamentos"
        });

        var client = new WebPushClient();
        var vapidDetails = new VapidDetails(
            _secrets.VapidSubject,
            _secrets.VapidPublicKey,
            _secrets.VapidPrivateKey);

        foreach (var subscription in subscriptions)
        {
            try
            {
                var pushSubscription = new PushSubscription(
                    subscription.Endpoint,
                    subscription.P256Dh,
                    subscription.Auth);

                await client.SendNotificationAsync(
                    pushSubscription,
                    payload,
                    vapidDetails,
                    cancellationToken);
            }
            catch (WebPushException ex) when (ex.StatusCode is HttpStatusCode.Gone or HttpStatusCode.NotFound)
            {
                await RemoveExpiredSubscriptionAsync(subscription.Endpoint, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao enviar notificacao push para {Endpoint}", subscription.Endpoint);
            }
        }
    }

    private async Task RemoveExpiredSubscriptionAsync(string endpoint, CancellationToken cancellationToken)
    {
        var subscription = await context.PushSubscriptionDevices
            .FirstOrDefaultAsync(x => x.Endpoint == endpoint, cancellationToken);

        if (subscription is null)
            return;

        context.PushSubscriptionDevices.Remove(subscription);
        await context.SaveChangesAsync(cancellationToken);
    }
}
