using BarberShop.Api.common.Api;
using BarberShop.Api.Models;
using BarberShop.Core.Requests.Account;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

public class UpdateProfileEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/profile", HandleAsync)
              .RequireAuthorization();

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal userClaims,
        UserManager<User> userManager,
        UpdateProfileRequest request)
    {
        var userId = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Results.NotFound("Usuário não encontrado");

        var resultEmail = await userManager.SetEmailAsync(user, request.Email);
        if (!resultEmail.Succeeded)
            return Results.BadRequest(resultEmail.Errors);

        var resultUserName = await userManager.SetUserNameAsync(user, request.Email);
        if (!resultUserName.Succeeded)
            return Results.BadRequest(resultUserName.Errors);

        user.NomeCompleto = request.Nome;
        user.PhoneNumber = request.Telefone;

        var resultUpdate = await userManager.UpdateAsync(user);
        if (!resultUpdate.Succeeded)
            return Results.BadRequest(resultUpdate.Errors);

        return Results.Ok("Perfil atualizado com sucesso!");
    }
}