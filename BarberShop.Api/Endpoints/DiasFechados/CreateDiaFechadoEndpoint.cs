using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.DiasFechados;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.DiasFechados;

namespace BarberShop.Api.Endpoints.DiasFechados
{
    public class CreateDiaFechadoEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapPost("/", HandleAsync)
                .WithName("DiasFechados: Create")
                .WithSummary("Cadastrar dia fechado")
                .WithDescription("Bloqueia uma data para agendamentos")
                .RequireAuthorization("Admin")
                .Produces<Response<DiaFechadoResponse?>>(201)
                .Produces<Response<DiaFechadoResponse?>>(400)
                .Produces<Response<DiaFechadoResponse?>>(500);

        private static async Task<IResult> HandleAsync(
            IDiaFechadoHandler handler,
            CreateDiaFechadoRequest request)
        {
            var result = await handler.CreateAsync(request);
            return result.IsSuccess
                ? Results.Created($"/{result.Data?.Id}", result)
                : Results.BadRequest(result);
        }
    }
}
