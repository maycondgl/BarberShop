using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Avaliacao;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Avaliacao;

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
            IAvaliacaoHandler handler,
            long id)
        {
            var request = new GetAvaliacaoByIdRequest
            {
                Id = id
            };
            var result = await handler.GetByIdAsync(request);
            return result.IsSuccess
              ? TypedResults.Ok(result)
              : TypedResults.BadRequest(result);
        }
    }
}
