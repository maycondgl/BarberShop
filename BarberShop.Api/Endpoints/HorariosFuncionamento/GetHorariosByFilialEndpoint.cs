using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.HorariosFuncionamento;
using BarberShop.Core.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Api.Endpoints.HorariosFuncionamento
{
    public class GetHorariosByFilialEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapGet("/{filialId}/horarios", HandleAsync)
                .WithName("HorariosFuncionamento: Get By Filial")
                .WithSummary("Obter horários de funcionamento da filial")
                .WithDescription("Recupera a grade semanal de dias, horários e almoço de uma filial")
                .Produces<Response<List<HorarioFuncionamentoDto>>>(200)
                .Produces<Response<List<HorarioFuncionamentoDto>>>(404)
                .Produces<Response<List<HorarioFuncionamentoDto>>>(500);

        private static async Task<IResult> HandleAsync(
            IHorarioFuncionamentoHandler handler,
            long filialId)
        {
            var result = await handler.GetByFilialAsync(filialId);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        }
    }
}
