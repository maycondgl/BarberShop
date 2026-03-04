using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Corte;

namespace BarberShop.Api.Endpoints.Cortes
{
    public class DeleteCorteEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
     => app.MapDelete("/{id}", HandleAsync)
         .WithName("Corte: Delete")
         .WithSummary("Exclui um corte")
         .WithDescription("Exclui um corte")
         .WithOrder(3)
         .Produces<Response<CorteResponse?>>(200)
         .Produces<Response<CorteResponse?>>(404)
         .Produces<Response<CorteResponse?>>(500);


        private static async Task<IResult> HandleAsync(
            ICorteHandler handler,
            long id)
        {
            var result = await handler.DeleteAsync(id);
            return result.IsSuccess
              ? TypedResults.Ok(result)
              : TypedResults.BadRequest(result);
        }
    }
}
