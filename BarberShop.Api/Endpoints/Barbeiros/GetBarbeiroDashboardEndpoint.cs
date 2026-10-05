using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Barbeiro;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BarberShop.Api.Endpoints.Barbeiros
{
    public class GetBarbeiroDashboardEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapGet("/dashboard", HandleAsync)
                .WithName("Barbeiros: Get Dashboard")
                .WithSummary("Obter dashboard do barbeiro")
                .WithDescription("Recupera métricas, faturamento, histórico e avaliações do barbeiro autenticado")
                .RequireAuthorization()
                .Produces<Response<BarbeiroDashboardResponse?>>(200)
                .Produces<Response<BarbeiroDashboardResponse?>>(404)
                .Produces<Response<BarbeiroDashboardResponse?>>(500);

        private static async Task<IResult> HandleAsync(
            ClaimsPrincipal user,
            IBarbeiroHandler handler,
            [FromQuery] long? barbeiroId = null)
        {
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            long? targetBarbeiroId = null;

            if (user.IsInRole("Admin") && barbeiroId.HasValue && barbeiroId.Value > 0)
            {
                targetBarbeiroId = barbeiroId.Value;
            }
            else if (long.TryParse(userIdClaim, out var userId))
            {
                var barbeiroUserResult = await handler.GetByUserIdAsync(userId);
                if (barbeiroUserResult.IsSuccess && barbeiroUserResult.Data is not null)
                {
                    targetBarbeiroId = barbeiroUserResult.Data.Id;
                }
                else if (barbeiroId.HasValue && barbeiroId.Value > 0)
                {
                    targetBarbeiroId = barbeiroId.Value;
                }
            }

            var result = await handler.GetDashboardAsync(targetBarbeiroId);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.Json(result, statusCode: result.Code > 0 ? result.Code : 500);
        }
    }
}
