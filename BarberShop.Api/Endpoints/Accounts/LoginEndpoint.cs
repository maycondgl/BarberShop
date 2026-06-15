using BarberShop.Api.common.Api;
using BarberShop.Api.Models;
using BarberShop.Core.Requests.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Endpoints.Accounts;

public class LoginEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/login", HandleAsync)
              .AllowAnonymous();

    private static async Task<IResult> HandleAsync(
        [FromQuery] bool? useCookies,
        [FromQuery] bool? useSessionCookies,
        [FromBody] LoginRequest body,
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        if (string.IsNullOrWhiteSpace(body.Email) || string.IsNullOrWhiteSpace(body.Senha))
            return Results.BadRequest("E-mail e senha são obrigatórios.");

        var email = body.Email.Trim();

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return Results.Unauthorized();

        if (!user.Ativo)
            return Results.Unauthorized();

        var passwordOk = await signInManager.CheckPasswordSignInAsync(
            user,
            body.Senha,
            lockoutOnFailure: false);

        if (!passwordOk.Succeeded)
            return Results.Unauthorized();

        var isPersistent = useSessionCookies != true;

        await signInManager.SignInAsync(
            user,
            isPersistent: isPersistent);

        return Results.Ok(new
        {
            message = "Login realizado com sucesso",
            user.Id,
            user.UserName,
            user.Email,
            user.PhoneNumber
        });
    }
}