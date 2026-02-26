using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Avaliacao;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Avaliacao;

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
            IAvaliacaoHandler handler,
            UpdateAvaliacaoRequest request,
            long id)
        {
            request.Id = id;

            var result = await handler.UpdateAsync(request);
            return result.IsSuccess
              ? TypedResults.Ok(result)
              : TypedResults.BadRequest(result);
        }
    }
}
