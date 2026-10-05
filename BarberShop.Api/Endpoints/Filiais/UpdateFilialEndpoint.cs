using BarberShop.Api.common.Api;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Filiais;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Filial;

namespace BarberShop.Api.Endpoints.Filiais
{
    public class UpdateFilialEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
            => app.MapPut("/{id}", HandleAsync)
                .WithName("Filiais: Update")
                .WithSummary("Atualizar filial")
                .WithDescription("Atualizar dados da filial")
                .WithOrder(2)
                .Produces<Response<FilialResponse?>>(200)
                .Produces<Response<FilialResponse?>>(400)
                .Produces<Response<FilialResponse?>>(404)
                .Produces<Response<FilialResponse?>>(500);

        private static async Task<IResult> HandleAsync(
            IFilialHandler handler,
            UpdateFilialRequest request,
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
