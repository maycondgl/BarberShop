using BarberShop.Api.Handlers;
using BarberShop.Core.Requests.Account;
using BarberShop.Api.common.Api;

namespace BarberShop.Api.Endpoints;

public class AccountEndpoints : IEndpoint 
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("v1/accounts")
            .WithTags("Account");

        group.MapPost("/register", HandleRegisterAsync);
    }

    private static async Task<IResult> HandleRegisterAsync(
        RegisterRequest request,
        AccountHandler handler)
    {
        var result = await handler.RegisterAsync(request);
        return result.IsSuccess ? Results.Created("", result) : Results.BadRequest(result);
    }
}