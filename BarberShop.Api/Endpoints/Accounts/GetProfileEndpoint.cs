using BarberShop.Api.common.Api;
using BarberShop.Api.Models;
using BarberShop.Core.Responses.Account;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BarberShop.Api.Endpoints.Accounts;

public class GetProfileEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/profile", HandleAsync)
              .RequireAuthorization()
              .WithName("Identity: Profile")
              .WithSummary("Obtém os dados do perfil do usuário logado");

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        UserManager<User> userManager)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var appUser = await userManager.FindByIdAsync(userId);

        if (appUser is null)
            return Results.NotFound("Usuário não encontrado");

        return Results.Ok(new ProfileResponse
        {
            Nome = appUser.NomeCompleto,
            Email = appUser.Email ?? string.Empty,
            Telefone = appUser.PhoneNumber ?? string.Empty
        });
    }
}