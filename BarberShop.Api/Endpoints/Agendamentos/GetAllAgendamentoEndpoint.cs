using BarberShop.Api.common.Api;
using BarberShop.Core;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Agendamento;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Endpoints.Agendamentos
{
    public class GetAllAgendamentoEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/", HandleAsync)
            .WithName("Agendamentos: Get All")
            .WithSummary("Recupera todos agendamentos")
            .WithDescription("Recupera todos serviços de cortes")
            .WithOrder(5)
            .Produces<PagedResponse<List<Agendamento>?>>(200)
            .Produces<Response<AgendamentoResponse?>>(404)
            .Produces<Response<AgendamentoResponse?>>(500);


        private static async Task<IResult> HandleAsync(
             IAgendamentoHandler handler,
             [FromQuery]int PageNumber,
             [FromQuery]int PageSize = Configuration.DefaultPageSize)
        {
            var request = new GetAllAgendamentoRequest
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
