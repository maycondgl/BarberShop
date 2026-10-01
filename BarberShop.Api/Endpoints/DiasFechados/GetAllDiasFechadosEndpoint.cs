using BarberShop.Api.common.Api;
using BarberShop.Core;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.DiasFechados;
using BarberShop.Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Endpoints.DiasFechados
{
    public class GetAllDiasFechadosEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapGet("/", HandleAsync)
                .WithName("DiasFechados: Get All")
                .WithSummary("Listar dias fechados")
                .WithDescription("Retorna as datas em que a barbearia não abrirá")
                .AllowAnonymous()
                .Produces<PagedResponse<List<DiaFechado>>>(200)
                .Produces<PagedResponse<List<DiaFechado>>>(500);

        private static async Task<IResult> HandleAsync(
            IDiaFechadoHandler handler,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = Configuration.DefaultPageSize)
        {
            var request = new GetAllDiasFechadosRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await handler.GetAllAsync(request);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        }
    }
}
