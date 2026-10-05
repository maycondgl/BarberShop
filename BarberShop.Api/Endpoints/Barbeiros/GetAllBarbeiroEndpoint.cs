using BarberShop.Api.common.Api;
using BarberShop.Core;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Barbeiros;
using BarberShop.Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Endpoints.Barbeiros
{
    public class GetAllBarbeiroEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapGet("/", HandleAsync)
                .WithName("Barbeiros: Get All")
                .WithSummary("Recupera todos os barbeiros")
                .WithDescription("Recupera a lista de barbeiros ativos, opcionalmente filtrados por filial")
                .WithOrder(5)
                .Produces<PagedResponse<List<Barbeiro>>>(200)
                .Produces<PagedResponse<List<Barbeiro>>>(500);

        private static async Task<IResult> HandleAsync(
            IBarbeiroHandler handler,
            [FromQuery] long? filialId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = Configuration.DefaultPageSize,
            [FromQuery] bool? apenasAtivos = null)
        {
            var request = new GetAllBarbeiroRequest
            {
                FilialId = filialId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                ApenasAtivos = apenasAtivos
            };

            var result = await handler.GetAllAsync(request);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        }
    }
}
