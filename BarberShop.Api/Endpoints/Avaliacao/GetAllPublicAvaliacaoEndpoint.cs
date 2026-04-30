using BarberShop.Api.common.Api;
using BarberShop.Core;
using BarberShop.Core.Handlers;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Avaliacao;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Endpoints.Avaliacao
{
    public class GetAllPublicAvaliacaoEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/", HandleAsync)
            .WithName("Avaliação: Get All Public")
            .WithSummary("Recupera todas avaliações publicamente")
            .AllowAnonymous()
            .WithOrder(6)
            .Produces<PagedResponse<List<AvaliacaoResponse>?>>(200)
            .Produces<Response<AvaliacaoResponse?>>(500);

        private static async Task<IResult> HandleAsync(
            IAvaliacaoHandler handler,
            [FromQuery] int PageNumber = 1,
            [FromQuery] int PageSize = Configuration.DefaultPageSize)
        {
            var result = await handler.GetAllPublicAsync(PageNumber, PageSize);

            Console.WriteLine($"Code: {result.TotalCount}, IsSuccess: {result.IsSuccess}, Msg: {result.Message}");

            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        }
    }
}