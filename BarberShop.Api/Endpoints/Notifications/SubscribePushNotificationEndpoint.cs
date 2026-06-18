using System.Security.Claims;
using BarberShop.Api.common.Api;
using BarberShop.Api.Data;
using BarberShop.Api.Models;
using BarberShop.Core.Requests.Notifications;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Api.Endpoints.Notifications;

public class SubscribePushNotificationEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/push-subscriptions", HandleAsync)
            .WithName("Notifications: Subscribe Push")
            .WithSummary("Cadastrar dispositivo do admin para notificacoes push")
            .Produces(200)
            .Produces(400)
            .Produces(401);

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        BarberShopContext context,
        PushSubscriptionRequest request)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Endpoint) ||
            string.IsNullOrWhiteSpace(request.P256Dh) ||
            string.IsNullOrWhiteSpace(request.Auth))
        {
            return Results.BadRequest("Inscricao push invalida.");
        }

        var subscription = await context.PushSubscriptionDevices
            .FirstOrDefaultAsync(x => x.Endpoint == request.Endpoint);

        if (subscription is null)
        {
            subscription = new PushSubscriptionDevice
            {
                UserId = userId,
                Endpoint = request.Endpoint,
                P256Dh = request.P256Dh,
                Auth = request.Auth
            };

            context.PushSubscriptionDevices.Add(subscription);
        }
        else
        {
            subscription.UserId = userId;
            subscription.P256Dh = request.P256Dh;
            subscription.Auth = request.Auth;
            subscription.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        return Results.Ok();
    }
}
