using BarberShop.Api.common.Api;
using BarberShop.Api.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BarberShop.Api.Endpoints.Account
{
    public class DeleteProfileEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapDelete("/profile", HandleAsync)
                  .RequireAuthorization();

        private static async Task<IResult> HandleAsync(
            ClaimsPrincipal userClaims,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            var userId = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);

            if (user is null)
                return Results.NotFound("Usuário não encontrado");

            await signInManager.SignOutAsync();

            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return Results.BadRequest(result.Errors);

            return Results.Ok("Conta excluída com sucesso!");
        }
    }
}