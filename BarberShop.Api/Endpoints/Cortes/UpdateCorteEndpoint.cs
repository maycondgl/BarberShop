using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Cortes;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Corte;

namespace BarberShop.Api.Endpoints.Cortes
{
    public class UpdateCorteEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapPut("/{id}", HandleAsync)
                .WithName("Corte: Update")
                .WithSummary("Atualiza um corte")
                .WithDescription("Atualiza um corte")
                .WithOrder(2)
                .Produces<Response<CorteResponse?>>(200)
                .Produces<Response<CorteResponse?>>(404)
                .Produces<Response<CorteResponse?>>(500);


        private static async Task<IResult> HandleAsync(
            ICorteHandler handler,
            UpdateCorteRequest request,
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
