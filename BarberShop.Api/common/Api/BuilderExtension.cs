using BarberShop.Api.Data;
using BarberShop.Api.Handlers;
using BarberShop.Api.Models;
using BarberShop.Core;
using BarberShop.Core.Handlers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Api.common.Api
{
    public static class BuilderExtension
    {
        public static void AddConfiguration(
            this WebApplicationBuilder builder)
        {
            Configuration.Connection = 
               builder
                .Configuration
                .GetConnectionString("Connection") 
               ?? string.Empty; 
            Configuration.BackendUrl = builder.Configuration.GetValue<string>("BackendUrl") ?? string.Empty;
            Configuration.FrontendUrl = builder.Configuration.GetValue<string>("FrontendUrl") ?? string.Empty;

        }

        public static void AddDocumentation(this WebApplicationBuilder builder) 
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(x =>
            {
                x.CustomSchemaIds(n => n.FullName);
            });
        }

        public static void AddSecurity(this WebApplicationBuilder builder)
        {
            builder.Services
            .AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies();

            builder.Services.AddAuthorization();
        }

        public static void AddDataContexts(this WebApplicationBuilder builder)
        {
            builder
                .Services
                .AddDbContext<BarberShopContext>(
                x =>
                {
                    x.UseSqlServer(Configuration.Connection);
                });
            builder.Services
                .AddIdentityCore<User>()
                .AddRoles<IdentityRole<long>>()
                .AddEntityFrameworkStores<BarberShopContext>()
                .AddApiEndpoints();
        }

        public static void AddCors(this WebApplicationBuilder builder)
        {
            builder.Services.AddCors(
                options => options.AddPolicy(
                    ApiConfiguration.CorsPolicyName,
                        policy => policy
                        .WithOrigins([
                            Configuration.BackendUrl,
                            Configuration.FrontendUrl
                            ])
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                    ));
        }

        public static void AddServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IAgendamentoHandler, AgendamentoHandler>();
            builder.Services.AddTransient<IAvaliacaoHandler, AvaliacaoHandler>();
            builder.Services.AddTransient<ICorteHandler, CorteHandler>();
        }  

    }
}
