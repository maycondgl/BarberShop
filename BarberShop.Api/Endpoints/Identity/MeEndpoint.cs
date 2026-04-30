using BarberShop.Api.common.Api;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BarberShop.Api.Endpoints.Identity
{
    public class MeEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapGet("/me", HandleAsync)
                  .RequireAuthorization();

        private static IResult HandleAsync(ClaimsPrincipal user)
        {
            return Results.Ok(new
            {
                Name = user.Identity?.Name,
                IsAuthenticated = user.Identity?.IsAuthenticated,
                Claims = user.Claims.Select(x => new { x.Type, x.Value })
            });
        }
    }
}