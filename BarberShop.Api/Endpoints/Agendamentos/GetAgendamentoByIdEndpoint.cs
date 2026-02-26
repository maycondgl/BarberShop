using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Agendamento;

namespace BarberShop.Api.Endpoints.Agendamentos
{
    public class GetAgendamentoByIdEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{id}", HandleAsync)
            .WithName("Agendamentos: Get By Id")
            .WithSummary("Recupera um agendamento")
            .WithDescription("Recupera um serviço de corte")
            .WithOrder(4)
            .Produces<Response<AgendamentoResponse?>>(200)
            .Produces<Response<AgendamentoResponse?>>(404)
            .Produces<Response<AgendamentoResponse?>>(500);


        private static async Task<IResult> HandleAsync(
            IAgendamentoHandler handler,
            long id)
        {
            var request = new GetAgendamentoByIdRequest
            {
                Id = id
            };
            var result = await handler.GetByIdAsync(request);
            return result.IsSuccess
              ? TypedResults.Ok(result)
              : TypedResults.BadRequest(result);
        }
    }
}
