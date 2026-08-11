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
            Configuration.Connection = builder.Configuration.GetConnectionString("Connection")?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Configuration.Connection))
                throw new InvalidOperationException(
                    "Connection string 'Connection' não encontrada. Configure-a com user-secrets localmente ou nas variáveis do ambiente de produção.");

            Configuration.BackendUrl = GetRequiredHttpUrl(builder.Configuration, "BackendUrl");
            Configuration.FrontendUrl = GetRequiredHttpUrl(builder.Configuration, "FrontendUrl");
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
            var extraOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();

            var allowedOrigins = extraOrigins
                .Append(Configuration.FrontendUrl)
                .Where(origin => !string.IsNullOrWhiteSpace(origin))
                .Select(origin => origin.Trim().TrimEnd('/'))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var origin in allowedOrigins)
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    throw new InvalidOperationException(
                        $"Origem CORS inválida: '{origin}'. Use uma URL HTTP ou HTTPS absoluta.");
                }
            }

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(ApiConfiguration.CorsPolicyName, policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
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

        private static string GetRequiredHttpUrl(IConfiguration configuration, string key)
        {
            var value = configuration.GetValue<string>(key)?.Trim().TrimEnd('/');

            if (string.IsNullOrWhiteSpace(value) ||
                !Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException(
                    $"Configuração '{key}' ausente ou inválida. Informe uma URL HTTP ou HTTPS absoluta.");
            }

            return value;
        }

    }
}
