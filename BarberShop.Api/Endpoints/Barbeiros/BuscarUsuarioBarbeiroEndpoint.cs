using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Barbeiro;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Endpoints.Barbeiros
{
    public class BuscarUsuarioBarbeiroEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapGet("/buscar-usuario", HandleAsync)
                .WithName("Barbeiros: Buscar Usuario Por Email")
                .WithSummary("Buscar usuário cadastrado por e-mail para vinculação como barbeiro")
                .RequireAuthorization("Admin")
                .Produces<Response<BarbeiroUsuarioInfoResponse?>>(200)
                .Produces<Response<BarbeiroUsuarioInfoResponse?>>(400)
                .Produces<Response<BarbeiroUsuarioInfoResponse?>>(404);

        private static async Task<IResult> HandleAsync(
            IBarbeiroHandler handler,
            [FromQuery] string email)
        {
            var result = await handler.BuscarUsuarioPorEmailAsync(email);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.Json(result, statusCode: result.Code > 0 ? result.Code : 400);
        }
    }
}
