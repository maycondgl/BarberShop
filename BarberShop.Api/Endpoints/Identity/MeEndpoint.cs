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
            ClaimsPrincipal user,
            UserManager<User> userManager)
        {
            var claims = user.Claims
                .Select(x => new { Type = x.Type, Value = x.Value })
                .ToList();

            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userIdClaim))
            {
                var dbUser = await userManager.FindByIdAsync(userIdClaim);
                if (dbUser != null)
                {
                    var roles = await userManager.GetRolesAsync(dbUser);
                    foreach (var role in roles)
                    {
                        if (!claims.Any(c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == role))
                        {
                            claims.Add(new { Type = ClaimTypes.Role, Value = role });
                            claims.Add(new { Type = "role", Value = role });
                        }
                    }
                }
            }

            return Results.Ok(new
            {
                Name = user.Identity?.Name,
                IsAuthenticated = user.Identity?.IsAuthenticated,
                Claims = claims
            });
        }
    }
}