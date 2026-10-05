using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Responses;

namespace BarberShop.Api.Endpoints.Barbeiros
{
    public class GetBarbeiroByUserIdEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapGet("/usuario/{userId:long}", HandleAsync)
                .WithName("Barbeiros: Get By User Id")
                .WithSummary("Obter barbeiro por ID de usuário")
                .WithDescription("Recupera os detalhes de um barbeiro vinculado a um usuário do sistema")
                .RequireAuthorization()
                .Produces<Response<Barbeiro?>>(200)
                .Produces<Response<Barbeiro?>>(404)
                .Produces<Response<Barbeiro?>>(500);

        private static async Task<IResult> HandleAsync(
            IBarbeiroHandler handler,
            long userId)
        {
            var result = await handler.GetByUserIdAsync(userId);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.NotFound(result);
        }
    }
}
