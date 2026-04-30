using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Agendamento;

public class UpdateStatusAgendamentoEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPatch("/{id}/status", HandleAsync)
            .WithName("Agendamento: Update Status")
                .WithSummary("Atualiza o status de um agendamento")
                .RequireAuthorization()
                .WithOrder(8)
                .Produces<Response<AgendamentoResponse?>>(200)
                .Produces<Response<AgendamentoResponse?>>(404)
                .Produces<Response<AgendamentoResponse?>>(500);

    private static async Task<IResult> HandleAsync(
        long id,
        UpdateStatusAgendamentoRequest request,
        IAgendamentoHandler handler)
    {
        request.Id = id;
        var result = await handler.UpdateStatusAsync(request);
        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}