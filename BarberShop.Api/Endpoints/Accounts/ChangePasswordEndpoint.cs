using BarberShop.Api.common.Api;
using BarberShop.Api.Models;
using BarberShop.Core.Requests.Account;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BarberShop.Api.Endpoints.Accounts;

public class ChangePasswordEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/change-password", HandleAsync)
            .RequireAuthorization()
            .WithName("Identity: Alterar Senha");

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        UserManager<User> userManager,
        ChangePasswordRequest request)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var appUser = await userManager.FindByIdAsync(userId);

        if (appUser is null)
            return Results.NotFound();

        if (request.NewPassword != request.ConfirmPassword)
            return Results.BadRequest("As senhas não coincidem");

        var result = await userManager.ChangePasswordAsync(
            appUser,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);

        return Results.Ok(new
        {
            message = "Senha alterada com sucesso"
        });
    }
}