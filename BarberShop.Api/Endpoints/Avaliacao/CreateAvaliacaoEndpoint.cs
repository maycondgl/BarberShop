using BarberShop.Api.common.Api;
using BarberShop.Api.Models;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Avaliacao;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Avaliacao;
using System.Security.Claims;

namespace BarberShop.Api.Endpoints.Avaliacao
{
    public class CreateAvaliacaoEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/", HandleAsync)
            .WithName("Avaliações: Create")
            .WithSummary("Avaliar um novo corte")
            .WithDescription("Avaliar um novo serviço de corte")
            .WithOrder(1)
            .Produces<Response<AvaliacaoResponse?>>(200)
            .Produces<Response<AvaliacaoResponse?>>(404)
            .Produces<Response<AvaliacaoResponse?>>(500);

        private static async Task<IResult> HandleAsync(
            ClaimsPrincipal user,
            IAvaliacaoHandler handler,
            CreateAvaliacaoRequest request)
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
