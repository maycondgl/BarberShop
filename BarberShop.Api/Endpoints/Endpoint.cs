using BarberShop.Api.common.Api;
using BarberShop.Api.Endpoints.Agendamentos;
using BarberShop.Core.Models;

namespace BarberShop.Api.Endpoints
{
    public static class Endpoint
    {
        public static void MapEndpoints(this WebApplication app)
        {
            var endpoints = app
                .MapGroup("");

            endpoints.MapGroup("v1/agendamentos")
                .WithTags("Agendamentos")
                //.RequireAuthorization()
                .MapEndpoint<CreateAgendamentoEndpoint>()
                .MapEndpoint<UpdateAgendamentoEndpoint>()
                .MapEndpoint<DeleteAgendamentoEndpoint>()
                .MapEndpoint<GetAgendamentoByIdEndpoint>()
                .MapEndpoint<GetAllAgendamentoEndpoint>();

        }
        private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
            where TEndpoint : IEndpoint
        {
            TEndpoint.Map(app);
            return app;
        }
    }
}
