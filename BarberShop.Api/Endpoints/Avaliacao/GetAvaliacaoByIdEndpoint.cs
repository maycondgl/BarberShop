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
    public class GetAvaliacaoByIdEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{id}", HandleAsync)
            .WithName("Avaliação: Get By Id")
            .WithSummary("Recupera uma avaliação")
            .WithDescription("Recupera uma avaliação")
            .WithOrder(4)
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

            var request = new GetAvaliacaoByIdRequest
            {
                UserId = userId,
                Id = id
            };
            var result = await handler.GetByIdAsync(request);
            return result.IsSuccess
              ? TypedResults.Ok(result)
              : TypedResults.BadRequest(result);
        }
    }
}
