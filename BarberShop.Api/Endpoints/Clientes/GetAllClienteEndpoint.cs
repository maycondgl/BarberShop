using BarberShop.Api.common.Api;
using BarberShop.Core;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Clientes;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Cliente;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Endpoints.Clientes
{
    public class GetAllClienteEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
       => app.MapGet("/", HandleAsync)
           .WithName("Cliente: Get All")
           .WithSummary("Recupera todos os clientes")
           .WithDescription("Recupera todos os clientes")
           .WithOrder(5)
           .Produces<PagedResponse<List<ClienteResponse>?>>(200)
           .Produces<Response<ClienteResponse?>>(404)
           .Produces<Response<ClienteResponse?>>(500);


        private static async Task<IResult> HandleAsync(
             IClienteHandler handler,
             [FromQuery] int PageNumber,
             [FromQuery] int PageSize = Configuration.DefaultPageSize)
        {
            var request = new GetAllClienteRequest
            {
                UserId = "Sistema",
                PageNumber = PageNumber,
                PageSize = PageSize
            };
            var result = await handler.GetAllAsync(request);
            return result.IsSuccess
              ? TypedResults.Ok(result)
              : TypedResults.BadRequest(result);
        }
    }
}
