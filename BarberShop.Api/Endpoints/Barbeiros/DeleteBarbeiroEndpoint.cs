using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Barbeiro;

namespace BarberShop.Api.Endpoints.Barbeiros
{
    public class DeleteBarbeiroEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapDelete("/{id}", HandleAsync)
                .WithName("Barbeiros: Delete")
                .WithSummary("Excluir barbeiro")
                .WithDescription("Remover um barbeiro")
                .WithOrder(3)
                .Produces<Response<BarbeiroResponse?>>(200)
                .Produces<Response<BarbeiroResponse?>>(400)
                .Produces<Response<BarbeiroResponse?>>(404)
                .Produces<Response<BarbeiroResponse?>>(500);

        private static async Task<IResult> HandleAsync(
            IBarbeiroHandler handler,
            long id)
        {
            var result = await handler.DeleteAsync(id);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        }
    }
}
