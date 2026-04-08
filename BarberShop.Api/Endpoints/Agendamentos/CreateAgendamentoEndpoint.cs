using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Agendamento;
using System.Security.Claims;

namespace BarberShop.Api.Endpoints.Agendamentos
{
    public class CreateAgendamentoEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/", HandleAsync)
            .WithName("Agendamentos: Create")
            .WithSummary("Criar um novo agendamento")
            .WithDescription("Criar um novo serviço de corte")
            .WithOrder(1)
            .Produces<Response<AgendamentoResponse?>>(200)
            .Produces<Response<AgendamentoResponse?>>(404)
            .Produces<Response<AgendamentoResponse?>>(500);

        private static async Task<IResult> HandleAsync(
            ClaimsPrincipal user,
            IAgendamentoHandler handler,
            CreateAgendamentoRequest request)
        {
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                return Results.Unauthorized();

            if (!long.TryParse(userIdClaim, out var userId))
                return Results.BadRequest("UserId inválido.");

            request.UserId = userId;

            var result = await handler.CreateAsync(request);
            return result.IsSuccess
              ? Results.Created($"/{result.Data?.Id}", result)
              : Results.BadRequest(result);
        }
    }
}
