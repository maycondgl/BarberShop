using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Agendamentos;

namespace BarberShop.Api.Endpoints.Agendamentos
{
    public class UpdateAgendamentoEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/{id}", HandleAsync)
            .WithName("Agendamentos: Update")
            .WithSummary("Atualiza um agendamento")
            .WithDescription("Atualiza um serviço de corte")
            .WithOrder(2)
            .Produces<Response<AgendamentoResponse?>>(200)
            .Produces<Response<AgendamentoResponse?>>(404)
            .Produces<Response<AgendamentoResponse?>>(500);


        private static async Task<IResult> HandleAsync(
            IAgendamentoHandler handler,
            UpdateAgendamentoRequest request,
            long id)
        {
            request.Id = id;
            


            var result = await handler.UpdateAsync(request);
            return result.IsSuccess
              ? TypedResults.Ok(result)
              : TypedResults.BadRequest(result);
        }
    }
}
