using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Dashboard;

namespace BarberShop.Api.Endpoints.Admin;

public class GetDashboardEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/dashboard", HandleAsync)
            .WithName("Admin: Get Dashboard")
            .WithSummary("Recupera métricas e gráficos do painel de administração")
            .RequireAuthorization("Admin")
            .Produces<Response<DashboardResponse?>>(200)
            .Produces<Response<DashboardResponse?>>(401)
            .Produces<Response<DashboardResponse?>>(403)
            .Produces<Response<DashboardResponse?>>(500);

    private static async Task<IResult> HandleAsync(IDashboardHandler handler)
    {
        var result = await handler.GetDashboardAsync();
        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}
