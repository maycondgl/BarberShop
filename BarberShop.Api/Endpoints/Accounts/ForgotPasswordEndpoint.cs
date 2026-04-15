using BarberShop.Api.common.Api;
using BarberShop.Api.Handlers;


namespace BarberShop.Api.Endpoints.Accounts;

public class ForgotPasswordEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/forgot-password", HandleAsync);

    private static async Task<IResult> HandleAsync(string email, AccountHandler handler)
    {
        var result = await handler.ForgotPasswordAsync(email);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}