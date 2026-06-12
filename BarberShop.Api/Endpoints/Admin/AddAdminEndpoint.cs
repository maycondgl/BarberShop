using BarberShop.Api.common.Api;
using BarberShop.Api.Handlers;
using BarberShop.Core.Responses;

namespace BarberShop.Api.Endpoints.Admin
{
    public class AddAdminEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapPost("/usuarios/{userId:long}/admin", HandleAsync)
                .WithName("Admin: Add User Admin")
                .WithSummary("Promove usuário a administrador")
                .RequireAuthorization("Admin")
                .Produces<Response<string?>>(200)
                .Produces<Response<string?>>(400)
                .Produces<Response<string?>>(401)
                .Produces<Response<string?>>(403)
                .Produces<Response<string?>>(404);

        private static async Task<IResult> HandleAsync(long userId, AccountHandler handler)
        {
            var result = await handler.AddAdminAsync(userId);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        }
    }
}
