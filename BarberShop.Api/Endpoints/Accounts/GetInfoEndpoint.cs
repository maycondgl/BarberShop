using BarberShop.Api.common.Api;
using BarberShop.Api.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BarberShop.Api.Endpoints.Accounts;

public class GetInfoEndpoint : IEndpoint 
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/manage/info", HandleAsync);

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal claimsPrincipal,
        UserManager<User> userManager)
    {
        var user = await userManager.GetUserAsync(claimsPrincipal);
        if (user is null) return Results.NotFound();

        return Results.Ok(new
        {
            user.Email,
            IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user)
        });
    }
}