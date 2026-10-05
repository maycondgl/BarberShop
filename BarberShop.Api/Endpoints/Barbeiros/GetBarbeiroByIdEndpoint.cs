using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Barbeiros;
using BarberShop.Core.Responses;

namespace BarberShop.Api.Endpoints.Barbeiros
{
    public class GetBarbeiroByIdEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapGet("/{id}", HandleAsync)
                .WithName("Barbeiros: Get By Id")
                .WithSummary("Obter barbeiro por ID")
                .WithDescription("Recupera os detalhes de um barbeiro")
                .WithOrder(4)
                .Produces<Response<Barbeiro?>>(200)
                .Produces<Response<Barbeiro?>>(404)
                .Produces<Response<Barbeiro?>>(500);

        private static async Task<IResult> HandleAsync(
            IBarbeiroHandler handler,
            long id)
        {
            var request = new GetBarbeiroByIdRequest { Id = id };
            var result = await handler.GetByIdAsync(request);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.NotFound(result);
        }
    }
}
