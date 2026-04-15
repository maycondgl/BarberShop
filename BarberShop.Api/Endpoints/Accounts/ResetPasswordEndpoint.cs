using BarberShop.Api.common.Api;
using BarberShop.Api.Handlers;
using BarberShop.Core.Requests.Account;

namespace BarberShop.Api.Endpoints.Accounts;

public class ResetPasswordEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/reset-password", HandleAsync);

    private static async Task<IResult> HandleAsync(ResetPasswordRequest request, AccountHandler handler)
    {
        var result = await handler.ResetPasswordAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}