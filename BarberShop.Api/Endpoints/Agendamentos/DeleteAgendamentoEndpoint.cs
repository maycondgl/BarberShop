using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Agendamento;
using System.Security.Claims;

namespace BarberShop.Api.Endpoints.Agendamentos
{
    public class DeleteAgendamentoEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/{id}", HandleAsync)
            .WithName("Agendamentos: Delete")
            .WithSummary("Exclui um agendamento")
            .WithDescription("Exclui um serviço de corte")
            .RequireAuthorization()
            .WithOrder(3)
            .Produces<Response<AgendamentoResponse?>>(200)
            .Produces<Response<AgendamentoResponse?>>(401)
            .Produces<Response<AgendamentoResponse?>>(403)
            .Produces<Response<AgendamentoResponse?>>(404)
            .Produces<Response<AgendamentoResponse?>>(500);


        private static async Task<IResult> HandleAsync(
            ClaimsPrincipal user,
            IAgendamentoHandler handler,
            long id)
        {
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!long.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var agendamentoResult = await handler.GetByIdAsync(new GetAgendamentoByIdRequest { Id = id });
            if (!agendamentoResult.IsSuccess || agendamentoResult.Data is null)
                return TypedResults.NotFound(new Response<AgendamentoResponse?>(null, 404, "Agendamento não encontrado"));

            if (agendamentoResult.Data.UserId != userId)
                return TypedResults.BadRequest(new Response<AgendamentoResponse?>(null, 403, "Você não pode excluir este agendamento"));


            var result = await handler.DeleteAsync(id);
            return result.IsSuccess
              ? TypedResults.Ok(result)
              : TypedResults.BadRequest(result);
        }
    }
}
