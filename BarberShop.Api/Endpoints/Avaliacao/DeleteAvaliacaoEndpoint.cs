using BarberShop.Api.common.Api;
using BarberShop.Api.Models;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Requests.Avaliacao;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Avaliacao;
using System.Security.Claims;

namespace BarberShop.Api.Endpoints.Avaliacao
{
    public class DeleteAvaliacaoEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
     => app.MapDelete("/{id}", HandleAsync)
         .WithName("Avaliação: Delete")
         .WithSummary("Exclui uma avaliação")
         .WithDescription("Exclui uma avaliação")
         .WithOrder(3)
         .Produces<Response<AvaliacaoResponse?>>(200)
         .Produces<Response<AvaliacaoResponse?>>(404)
         .Produces<Response<AvaliacaoResponse?>>(500);


        private static async Task<IResult> HandleAsync(
            ClaimsPrincipal user,
            IAvaliacaoHandler handler,
            long id)
        {
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!long.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var request = new DeleteAvaliacaoRequest
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
