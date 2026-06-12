using BarberShop.Api.common.Api;
using BarberShop.Api.Handlers;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Account;

namespace BarberShop.Api.Endpoints.Admin
{
    public class GetUsersEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapGet("/usuarios", HandleAsync)
                .WithName("Admin: Get Users")
                .WithSummary("Lista usuários")
                .RequireAuthorization("Admin")
                .Produces<Response<List<AdminUserResponse>>>(200)
                .Produces<Response<List<AdminUserResponse>>>(401)
                .Produces<Response<List<AdminUserResponse>>>(403);

        private static async Task<IResult> HandleAsync(AccountHandler handler)
        {
            var result = await handler.GetUsersAsync();
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        }
    }
}
