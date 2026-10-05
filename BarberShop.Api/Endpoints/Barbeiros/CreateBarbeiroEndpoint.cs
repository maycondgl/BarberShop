using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Barbeiros;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Barbeiro;

namespace BarberShop.Api.Endpoints.Barbeiros
{
    public class CreateBarbeiroEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapPost("/", HandleAsync)
                .WithName("Barbeiros: Create")
                .WithSummary("Novo barbeiro")
                .WithDescription("Cadastrar um novo barbeiro")
                .WithOrder(1)
                .Produces<Response<BarbeiroResponse?>>(201)
                .Produces<Response<BarbeiroResponse?>>(400)
                .Produces<Response<BarbeiroResponse?>>(500);

        private static async Task<IResult> HandleAsync(
            IBarbeiroHandler handler,
            CreateBarbeiroRequest request)
        {
            var result = await handler.CreateAsync(request);
            return result.IsSuccess
                ? Results.Created($"/{result.Data?.Id}", result)
                : Results.BadRequest(result);
        }
    }
}
