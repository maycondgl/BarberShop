using BarberShop.Api.common.Api;
using BarberShop.Api.Handlers;
using BarberShop.Core.Requests.Account;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Endpoints.Accounts;

public class LoginEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/login", HandleAsync);

    private static async Task<IResult> HandleAsync(
    [FromQuery] bool? useCookies,     
    [FromQuery] bool? useSessionCookies,  
    [FromBody] LoginRequest body,      
    AccountHandler handler)
    {
        var meuRequest = new LoginRequest
        {
            Email = body.Email,
            Senha = body.Senha
        };

        var result = await handler.LoginAsync(meuRequest);
        return result.IsSuccess ? Results.Ok(result) : Results.Unauthorized();
    }

}