using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.DiasFechados;

namespace BarberShop.Api.Endpoints.DiasFechados
{
    public class DeleteDiaFechadoEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapDelete("/{id:long}", HandleAsync)
                .WithName("DiasFechados: Delete")
                .WithSummary("Remover dia fechado")
                .WithDescription("Desbloqueia uma data previamente fechada")
                .RequireAuthorization("Admin")
                .Produces<Response<DiaFechadoResponse?>>(200)
                .Produces<Response<DiaFechadoResponse?>>(404)
                .Produces<Response<DiaFechadoResponse?>>(500);

        private static async Task<IResult> HandleAsync(
            IDiaFechadoHandler handler,
            long id)
        {
            var result = await handler.DeleteAsync(id);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        }
    }
}
