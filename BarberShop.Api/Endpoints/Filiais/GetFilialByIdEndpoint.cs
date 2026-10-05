using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Filiais;
using BarberShop.Core.Responses;

namespace BarberShop.Api.Endpoints.Filiais
{
    public class GetFilialByIdEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapGet("/{id}", HandleAsync)
                .WithName("Filiais: Get By Id")
                .WithSummary("Obter filial por ID")
                .WithDescription("Recupera os detalhes de uma filial")
                .WithOrder(4)
                .Produces<Response<Filial?>>(200)
                .Produces<Response<Filial?>>(404)
                .Produces<Response<Filial?>>(500);

        private static async Task<IResult> HandleAsync(
            IFilialHandler handler,
            long id)
        {
            var request = new GetFilialByIdRequest { Id = id };
            var result = await handler.GetByIdAsync(request);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.NotFound(result);
        }
    }
}
