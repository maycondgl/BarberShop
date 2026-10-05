using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.HorariosFuncionamento;
using BarberShop.Core.Responses;
using System.Threading.Tasks;

namespace BarberShop.Api.Endpoints.HorariosFuncionamento
{
    public class SaveHorariosFilialEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapPut("/{filialId}/horarios", HandleAsync)
                .WithName("HorariosFuncionamento: Save By Filial")
                .WithSummary("Atualizar horários de funcionamento da filial")
                .WithDescription("Salva a grade semanal de dias, horários e almoço de uma filial")
                .RequireAuthorization("Admin")
                .Produces<Response<bool>>(200)
                .Produces<Response<bool>>(400)
                .Produces<Response<bool>>(404)
                .Produces<Response<bool>>(500);

        private static async Task<IResult> HandleAsync(
            IHorarioFuncionamentoHandler handler,
            long filialId,
            SalvarHorariosFilialRequest request)
        {
            request.FilialId = filialId;
            var result = await handler.SaveHorariosAsync(request);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        }
    }
}
