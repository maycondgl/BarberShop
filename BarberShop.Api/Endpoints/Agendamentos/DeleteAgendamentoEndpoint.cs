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
            .WithOrder(3)
            .Produces<Response<AgendamentoResponse?>>(200)
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

            var request = new DeleteAgendamentoRequest
            {
                UserId = userId,
                Id = id
            };
            var result = await handler.DeleteAsync(id);
            return result.IsSuccess
              ? TypedResults.Ok(result)
              : TypedResults.BadRequest(result);
        }
    }
}
