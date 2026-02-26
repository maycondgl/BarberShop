using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Avaliacao;

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
            IAvaliacaoHandler handler,
            long id)
        {
            var result = await handler.DeleteAsync(id);
            return result.IsSuccess
              ? TypedResults.Ok(result)
              : TypedResults.BadRequest(result);
        }
    }
}
