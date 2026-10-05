using BarberShop.Api.common.Api;
using BarberShop.Core;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Filiais;
using BarberShop.Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Endpoints.Filiais
{
    public class GetAllFilialEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapGet("/", HandleAsync)
                .WithName("Filiais: Get All")
                .WithSummary("Recupera todas as filiais")
                .WithDescription("Recupera a lista de filiais ativas")
                .WithOrder(5)
                .Produces<PagedResponse<List<Filial>>>(200)
                .Produces<PagedResponse<List<Filial>>>(500);

        private static async Task<IResult> HandleAsync(
            IFilialHandler handler,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = Configuration.DefaultPageSize,
            [FromQuery] bool? apenasAtivos = null)
        {
            var request = new GetAllFilialRequest
            {
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
