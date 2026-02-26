using BarberShop.Api.common.Api;
using BarberShop.Core;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Avaliacao;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Avaliacao;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Endpoints.Avaliacao
{
    public class GetAllAvaliacaoEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/", HandleAsync)
            .WithName("Avaliação: Get All")
            .WithSummary("Recupera todas Avaliação")
            .WithDescription("Recupera todas as Avaliações")
            .WithOrder(5)
            .Produces<PagedResponse<List<AvaliacaoResponse>?>>(200)
            .Produces<Response<AvaliacaoResponse?>>(404)
            .Produces<Response<AvaliacaoResponse?>>(500);


        private static async Task<IResult> HandleAsync(
             IAvaliacaoHandler handler,
             [FromQuery] int PageNumber,
             [FromQuery] int PageSize = Configuration.DefaultPageSize)
        {
            var request = new GetAllAvaliacaoRequest
            {
                UserId = "maycon",
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

