using BarberShop.Api.common.Api;
using BarberShop.Core;
using BarberShop.Core.Handlers;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Avaliacao;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Endpoints.Avaliacao
{
    public class GetAllAdminAvaliacoesEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapGet("/admin", HandleAsync)
                .WithName("Avaliações: Get All Admin")
                .WithSummary("Recupera todas as avaliações para o administrador")
                .RequireAuthorization("Admin")
                .Produces<PagedResponse<List<AvaliacaoResponse>>>(200)
                .Produces<PagedResponse<List<AvaliacaoResponse>>>(500);

        private static async Task<IResult> HandleAsync(
            IAvaliacaoHandler handler,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = Configuration.DefaultPageSize)
        {
            var result = await handler.GetAllAdminAsync(
                pageNumber <= 0 ? 1 : pageNumber,
                pageSize <= 0 ? Configuration.DefaultPageSize : pageSize);

            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        }
    }
}
