using BarberShop.Api.common.Api;
using BarberShop.Api.Models;
using BarberShop.Core.Responses.Notifications;
using Microsoft.Extensions.Options;

namespace BarberShop.Api.Endpoints.Notifications;

public class GetPushNotificationOptionsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/push-options", HandleAsync)
            .WithName("Notifications: Push Options")
            .WithSummary("Obter chave publica para notificacoes push")
            .Produces<PushNotificationOptionsResponse>(200);

    private static IResult HandleAsync(IOptions<Secrets> secretsOptions)
    {
        var publicKey = secretsOptions.Value.VapidPublicKey;
        return Results.Ok(new PushNotificationOptionsResponse(publicKey));
    }
}
