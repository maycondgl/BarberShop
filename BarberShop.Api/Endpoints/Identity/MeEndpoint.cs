using BarberShop.Api.common.Api;
using BarberShop.Api.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BarberShop.Api.Endpoints.Identity
{
    public class MeEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapGet("/me", HandleAsync)
                  .RequireAuthorization();

        private static async Task<IResult> HandleAsync(
            ClaimsPrincipal claims,
            UserManager<User> userManager)
        {
            var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);

            if (user is null)
                return Results.Unauthorized();

            if (!user.Ativo)
                return Results.Unauthorized();

            var roles = await userManager.GetRolesAsync(user);

            return Results.Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.PhoneNumber,
                user.Ativo,
                Roles = roles,
                IsAdmin = roles.Contains("Admin")
            });
        }
    }
}