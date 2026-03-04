using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Clientes;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Cliente;

namespace BarberShop.Api.Endpoints.Clientes
{
   public class UpdateClienteEndpoint : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app)
            => app.MapPut("/{id}", HandleAsync)
                .WithName("Cliente: Update")
                .WithSummary("Atualiza um cliente")
                .WithDescription("Atualiza um cliente criado")
                .WithOrder(2)
                .Produces<Response<ClienteResponse?>>(200)
                .Produces<Response<ClienteResponse?>>(404)
                .Produces<Response<ClienteResponse?>>(500);


            private static async Task<IResult> HandleAsync(
                IClienteHandler handler,
                UpdateClienteRequest request,
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
