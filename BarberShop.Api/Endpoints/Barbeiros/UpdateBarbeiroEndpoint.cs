using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Barbeiros;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Barbeiro;

namespace BarberShop.Api.Endpoints.Barbeiros
{
    public class UpdateBarbeiroEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapPut("/{id}", HandleAsync)
                .WithName("Barbeiros: Update")
                .WithSummary("Atualizar barbeiro")
                .WithDescription("Atualizar dados do barbeiro")
                .WithOrder(2)
                .Produces<Response<BarbeiroResponse?>>(200)
                .Produces<Response<BarbeiroResponse?>>(400)
                .Produces<Response<BarbeiroResponse?>>(404)
                .Produces<Response<BarbeiroResponse?>>(500);

        private static async Task<IResult> HandleAsync(
            IBarbeiroHandler handler,
            UpdateBarbeiroRequest request,
            long id)
        {
            request.Id = id;
            var result = await handler.UpdateAsync(request);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        }
    }
}
