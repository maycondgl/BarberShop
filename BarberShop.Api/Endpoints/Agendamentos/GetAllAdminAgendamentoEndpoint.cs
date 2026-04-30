using BarberShop.Api.common.Api;
using BarberShop.Core;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Agendamento;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Endpoints.Agendamentos
{
    public class GetAllAdminAgendamentoEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/admin", HandleAsync)
            .WithName("Agendamentos: Get All Admin")
            .WithSummary("Admin: Recupera todos agendamentos")
            .RequireAuthorization()
            .WithOrder(7)
            .Produces<PagedResponse<List<AgendamentoResponse>?>>(200)
            .Produces<Response<AgendamentoResponse?>>(500);

        private static async Task<IResult> HandleAsync(
            IAgendamentoHandler handler,
            [FromQuery] int PageNumber = 1,
            [FromQuery] int PageSize = Configuration.DefaultPageSize)
        {
            var request = new GetAllAgendamentoRequest
            {
                PageNumber = PageNumber,
                PageSize = PageSize
            };
            var result = await handler.GetAllAdminAsync(request);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        }
    }
}