using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Cliente;


namespace BarberShop.Api.Endpoints.Clientes
{
    public class DeleteClienteEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
     => app.MapDelete("/{id}", HandleAsync)
         .WithName("Cliente: Delete")
         .WithSummary("Exclui um cliente")
         .WithDescription("Exclui um cliente")
         .WithOrder(3)
         .Produces<Response<ClienteResponse?>>(200)
         .Produces<Response<ClienteResponse?>>(404)
         .Produces<Response<ClienteResponse?>>(500);


        private static async Task<IResult> HandleAsync(
            IClienteHandler handler,
            long id)
        {
            var result = await handler.DeleteAsync(id);
            return result.IsSuccess
              ? TypedResults.Ok(result)
              : TypedResults.BadRequest(result);
        }
    }
}
