using BarberShop.Api.common.Api;
using BarberShop.Core;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Cortes;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Corte;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Endpoints.Cortes
{
    public class GetAllCorteEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
      => app.MapGet("/", HandleAsync)
          .WithName("Corte: Get All")
          .WithSummary("Recupera todos os cortes")
          .WithDescription("Recupera todos os cortes")
          .WithOrder(5)
          .Produces<PagedResponse<List<CorteResponse>?>>(200)
          .Produces<Response<CorteResponse?>>(404)
          .Produces<Response<CorteResponse?>>(500);


        private static async Task<IResult> HandleAsync(
             ICorteHandler handler,
             [FromQuery] int PageNumber,
             [FromQuery] int PageSize = Configuration.DefaultPageSize)
        {
            var request = new GetAllCorteRequest
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
