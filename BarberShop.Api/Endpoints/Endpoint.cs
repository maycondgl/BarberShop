using BarberShop.Api.common.Api;
using BarberShop.Api.Endpoints.Account;
using BarberShop.Api.Endpoints.Accounts;
using BarberShop.Api.Endpoints.Agendamentos;
using BarberShop.Api.Endpoints.Avaliacao;
using BarberShop.Api.Endpoints.Cortes;
using BarberShop.Api.Endpoints.Identity;
using BarberShop.Api.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BarberShop.Api.Endpoints
{
    public static class Endpoint
    {
        public static void MapEndpoints(this WebApplication app)
        {
            var endpoints = app
                .MapGroup("");

            endpoints.MapGroup("/")
                .WithTags("Health Check")
                .MapGet("/", () => new { message = "OK" });

            endpoints.MapGroup("v1/agendamentos")
                .WithTags("Agendamentos")
                //.RequireAuthorization()
                .MapEndpoint<GetAllAdminAgendamentoEndpoint>()  
                .MapEndpoint<UpdateStatusAgendamentoEndpoint>()
                .MapEndpoint<CreateAgendamentoEndpoint>()
                .MapEndpoint<UpdateAgendamentoEndpoint>()
                .MapEndpoint<DeleteAgendamentoEndpoint>()
                .MapEndpoint<GetAgendamentoByPeriodEndpoint>()
                .MapEndpoint<GetAgendamentoByIdEndpoint>()
                .MapEndpoint<GetAllAgendamentoEndpoint>();

            endpoints.MapGroup("v1/public/avaliacoes")
                .WithTags("Avaliações Públicas")
                .AllowAnonymous()
                .MapEndpoint<GetAllPublicAvaliacaoEndpoint>();

            endpoints.MapGroup("v1/avaliacoes")
               .WithTags("Avaliações")
               //.RequireAuthorization()
               .MapEndpoint<CreateAvaliacaoEndpoint>()
               .MapEndpoint<UpdateAvaliacaoEndpoint>()
               .MapEndpoint<DeleteAvaliacaoEndpoint>()
               .MapEndpoint<GetAvaliacaoByIdEndpoint>()
               .MapEndpoint<GetAllAvaliacaoEndpoint>();
               

            endpoints.MapGroup("v1/cortes")
               .WithTags("cortes")
               //.RequireAuthorization()
               .MapEndpoint<CreateCorteEndpoint>()
               .MapEndpoint<UpdateCorteEndpoint>()
               .MapEndpoint<DeleteCorteEndpoint>()
               .MapEndpoint<GetCorteByIdEndpoint>()
               .MapEndpoint<GetAllCorteEndpoint>();



            endpoints.MapGroup("v1/identity")
                .WithTags("Identity")
                //MapIdentityApi<User>();

                .MapEndpoint<MeEndpoint>()
                .MapEndpoint<RegisterEndpoint>()
                .MapEndpoint<LoginEndpoint>()
                .MapEndpoint<LogoutEndpoint>() 
                .MapEndpoint<GetRolesEndpoint>()
                .MapEndpoint<ForgotPasswordEndpoint>() 
                .MapEndpoint<ResetPasswordEndpoint>()
                .MapEndpoint<GetInfoEndpoint>() 
                .MapEndpoint<TwoFactorEndpoint>()
                .MapEndpoint<UpdateProfileEndpoint>()
                .MapEndpoint<DeleteProfileEndpoint>()


            .MapPost("/refresh", (ClaimsPrincipal user, SignInManager<User> signInManager) => {
             });
        }
        private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
            where TEndpoint : IEndpoint
        {
            TEndpoint.Map(app);
            return app;
        }
    }
}
