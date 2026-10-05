using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Filial;

namespace BarberShop.Api.Endpoints.Filiais
{
    public class DeleteFilialEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapDelete("/{id}", HandleAsync)
                .WithName("Filiais: Delete")
                .WithSummary("Excluir filial")
                .WithDescription("Remover uma filial")
                .WithOrder(3)
                .Produces<Response<FilialResponse?>>(200)
                .Produces<Response<FilialResponse?>>(400)
                .Produces<Response<FilialResponse?>>(404)
                .Produces<Response<FilialResponse?>>(500);

        private static async Task<IResult> HandleAsync(
            IFilialHandler handler,
            long id)
        {
            var result = await handler.DeleteAsync(id);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        }
    }
}
