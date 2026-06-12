using BarberShop.Api.common.Api;
using BarberShop.Api.Handlers;
using BarberShop.Core.Responses;

namespace BarberShop.Api.Endpoints.Admin
{
    public class RemoveAdminEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapDelete("/usuarios/{userId:long}/admin", HandleAsync)
                .WithName("Admin: Remove User Admin")
                .WithSummary("Remove permissão de administrador")
                .RequireAuthorization("Admin")
                .Produces<Response<string?>>(200)
                .Produces<Response<string?>>(400)
                .Produces<Response<string?>>(401)
                .Produces<Response<string?>>(403)
                .Produces<Response<string?>>(404);

        private static async Task<IResult> HandleAsync(long userId, AccountHandler handler)
        {
            var result = await handler.RemoveAdminAsync(userId);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        }
    }
}
