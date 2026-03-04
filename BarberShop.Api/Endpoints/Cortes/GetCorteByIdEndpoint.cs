using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Cortes;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Corte;

namespace BarberShop.Api.Endpoints.Cortes
{
    public class GetCorteByIdEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{id}", HandleAsync)
            .WithName("Corte: Get By Id")
            .WithSummary("Recupera um corte")
            .WithDescription("Recupera um corte")
            .WithOrder(4)
            .Produces<Response<CorteResponse?>>(200)
            .Produces<Response<CorteResponse?>>(404)
            .Produces<Response<CorteResponse?>>(500);


        private static async Task<IResult> HandleAsync(
            ICorteHandler handler,
            long id)
        {
            var request = new GetCorteByIdRequest
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
