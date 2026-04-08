using BarberShop.Api.common.Api;
using BarberShop.Api.Models;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Avaliacao;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Avaliacao;
using System.Security.Claims;

namespace BarberShop.Api.Endpoints.Avaliacao
{
    public class UpdateAvaliacaoEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/{id}", HandleAsync)
            .WithName("Avaliação: Update")
            .WithSummary("Atualiza uma avaliação")
            .WithDescription("Atualiza uma avaliação de um serviço")
            .WithOrder(2)
            .Produces<Response<AvaliacaoResponse?>>(200)
            .Produces<Response<AvaliacaoResponse?>>(404)
            .Produces<Response<AvaliacaoResponse?>>(500);


        private static async Task<IResult> HandleAsync(
            ClaimsPrincipal user,
            IAvaliacaoHandler handler,
            UpdateAvaliacaoRequest request,
            long id)
        {
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!long.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            request.UserId = userId;
            request.Id = id;


            var result = await handler.UpdateAsync(request);
            return result.IsSuccess
              ? TypedResults.Ok(result)
              : TypedResults.BadRequest(result);
        }
    }
}
