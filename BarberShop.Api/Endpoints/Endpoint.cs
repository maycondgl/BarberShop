using BarberShop.Api.common.Api;
using BarberShop.Api.Endpoints.Agendamentos;
using BarberShop.Api.Endpoints.Avaliacao;

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

            endpoints.MapGroup("v1/avaliacoes")
               .WithTags("Avaliações")
               //.RequireAuthorization()
               .MapEndpoint<CreateAvaliacaoEndpoint>()
               .MapEndpoint<UpdateAvaliacaoEndpoint>()
               .MapEndpoint<DeleteAvaliacaoEndpoint>()
               .MapEndpoint<GetAvaliacaoByIdEndpoint>()
               .MapEndpoint<GetAllAvaliacaoEndpoint>();

        }
        private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
            where TEndpoint : IEndpoint
        {
            TEndpoint.Map(app);
            return app;
        }
    }
}
