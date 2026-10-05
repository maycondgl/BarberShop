using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Filiais;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Filial;

namespace BarberShop.Api.Endpoints.Filiais
{
    public class CreateFilialEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapPost("/", HandleAsync)
                .WithName("Filiais: Create")
                .WithSummary("Nova filial")
                .WithDescription("Cadastrar uma nova filial")
                .WithOrder(1)
                .Produces<Response<FilialResponse?>>(201)
                .Produces<Response<FilialResponse?>>(400)
                .Produces<Response<FilialResponse?>>(500);

        private static async Task<IResult> HandleAsync(
            IFilialHandler handler,
            CreateFilialRequest request)
        {
            var result = await handler.CreateAsync(request);
            return result.IsSuccess
                ? Results.Created($"/{result.Data?.Id}", result)
                : Results.BadRequest(result);
        }
    }
}
