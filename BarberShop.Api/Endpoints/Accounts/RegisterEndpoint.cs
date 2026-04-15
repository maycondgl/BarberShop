using BarberShop.Api.Handlers;
using BarberShop.Core.Requests.Account;
using BarberShop.Api.common.Api;

namespace BarberShop.Api.Endpoints.Accounts;

public class RegisterEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/register", HandleAsync);

    private static async Task<IResult> HandleAsync(
        RegisterRequest request,
        AccountHandler handler)
    {
        var result = await handler.RegisterAsync(request);
        return result.IsSuccess
            ? Results.Created("", result)
            : Results.BadRequest(result);
    }
}