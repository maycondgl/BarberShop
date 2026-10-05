using BarberShop.Api.common.Api;
using BarberShop.Core;
using BarberShop.Core.Handlers;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Avaliacao;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Endpoints.Avaliacao
{
    public class GetAvaliacoesByBarbeiroEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapGet("/barbeiro/{barbeiroId:long}", HandleAsync)
                .WithName("Avaliações: Get By Barbeiro")
                .WithSummary("Recupera avaliações de um barbeiro específico")
                .RequireAuthorization()
                .Produces<PagedResponse<List<AvaliacaoResponse>>>(200)
                .Produces<PagedResponse<List<AvaliacaoResponse>>>(500);

        private static async Task<IResult> HandleAsync(
            IAvaliacaoHandler handler,
            long barbeiroId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = Configuration.DefaultPageSize)
        {
            var result = await handler.GetByBarbeiroAsync(
                barbeiroId,
                pageNumber <= 0 ? 1 : pageNumber,
                pageSize <= 0 ? Configuration.DefaultPageSize : pageSize);

            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        }
    }
}
