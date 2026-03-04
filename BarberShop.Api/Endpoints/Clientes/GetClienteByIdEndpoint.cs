using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Clientes;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Cliente;

namespace BarberShop.Api.Endpoints.Clientes
{
    public class GetClienteByIdEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{id}", HandleAsync)
            .WithName("Cliente: Get By Id")
            .WithSummary("Recupera um cliente")
            .WithDescription("Recupera um cliente")
            .WithOrder(4)
            .Produces<Response<ClienteResponse?>>(200)
            .Produces<Response<ClienteResponse?>>(404)
            .Produces<Response<ClienteResponse?>>(500);


        private static async Task<IResult> HandleAsync(
            IClienteHandler handler,
            long id)
        {
            var request = new GetClienteByIdRequest
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
