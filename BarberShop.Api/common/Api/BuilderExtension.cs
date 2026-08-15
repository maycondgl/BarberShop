using BarberShop.Api.Data;
using BarberShop.Api.Handlers;
using BarberShop.Api.Models;
using BarberShop.Api.Services;
using BarberShop.Core;
using BarberShop.Core.Handlers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Api.common.Api
{
    public static class BuilderExtension
    {
        public static void AddConfiguration(this WebApplicationBuilder builder)
        {
            Configuration.Connection = DatabaseConnectionSettings.Resolve(builder.Configuration);

            Configuration.BackendUrl = builder.Configuration.GetValue<string>("BackendUrl") ?? string.Empty;
            Configuration.FrontendUrl = builder.Configuration.GetValue<string>("FrontendUrl") ?? string.Empty;
            Configuration.AdminSetupKey = builder.Configuration.GetValue<string>("AdminSetupKey") ?? string.Empty;

            builder.Services.Configure<Secrets>(
                builder.Configuration.GetSection("Secrets"));
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

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "BarberShop.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            builder.Services.Configure<CookieAuthenticationOptions>(
                IdentityConstants.ApplicationScheme,
                options =>
                {
                    options.Cookie.Name = "BarberShop.Auth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                    policy.RequireRole("Admin"));
            });

            builder.Services.AddIdentityCore<User>(options =>
            {
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddRoles<IdentityRole<long>>()
            .AddRoleManager<RoleManager<IdentityRole<long>>>()
            .AddSignInManager<SignInManager<User>>()
            .AddEntityFrameworkStores<BarberShopContext>()
            .AddDefaultTokenProviders()
            .AddApiEndpoints();
        }

        public static void AddDataContexts(this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<BarberShopContext>(options =>
            {
                options.UseSqlServer(Configuration.Connection);
            });
        }

        public static void AddCors(this WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(ApiConfiguration.CorsPolicyName, policy =>
                {
                    policy
                        .WithOrigins(
                            "https://barbershop-web-gwbhheaaf0cfewgm.centralus-01.azurewebsites.net",
                            "http://localhost:5252",
                            "https://localhost:5252"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
        }

        public static void AddServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddSignalR();
            builder.Services.AddTransient<IAgendamentoHandler, AgendamentoHandler>();
            builder.Services.AddTransient<IAvaliacaoHandler, AvaliacaoHandler>();
            builder.Services.AddTransient<ICorteHandler, CorteHandler>();
            builder.Services.AddTransient<AccountHandler>();
            builder.Services.AddScoped<IAgendamentoNotificationService, AgendamentoNotificationService>();
        }  

    }
}
