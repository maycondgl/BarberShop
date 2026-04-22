using BarberShop.Api.common.Api;
using BarberShop.Core;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Agendamento;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Endpoints.Agendamentos
{
    public class GetAgendamentoByPeriodEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapGet("/period", HandleAsync) 
                .WithName("Agendamentos: Get By Period")
                .WithSummary("Recupera agendamentos por período")
                .WithDescription("Recupera agendamentos dentro de um intervalo de datas")
                .WithOrder(5)
                .Produces<PagedResponse<List<AgendamentoResponse>?>>(200)
                .Produces<PagedResponse<List<AgendamentoResponse>?>>(400)
                .Produces<PagedResponse<List<AgendamentoResponse>?>>(500);
        private static async Task<IResult> HandleAsync(
            IAgendamentoHandler handler,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = Configuration.DefaultPageSize)
        {
            var request = new GetAgendamentoByPeriodRequest
            {
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await handler.GetByPeriodAsync(request);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        }
    }
}
