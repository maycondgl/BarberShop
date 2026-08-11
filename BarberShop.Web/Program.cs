using BarberShop.Core.Handlers;
using BarberShop.Web;
using BarberShop.Web.Handlers;
using BarberShop.Web.Security;
using BarberShop.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);

var backendUrl = builder.Configuration.GetValue<string>("BackendUrl")?.Trim().TrimEnd('/');

if (string.IsNullOrWhiteSpace(backendUrl) ||
    !Uri.TryCreate(backendUrl, UriKind.Absolute, out var backendUri) ||
    (backendUri.Scheme != Uri.UriSchemeHttp && backendUri.Scheme != Uri.UriSchemeHttps))
{
    throw new InvalidOperationException(
        "Configuração 'BackendUrl' ausente ou inválida. Informe uma URL HTTP ou HTTPS absoluta.");
}

Configuration.BackendUrl = backendUrl;

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<CookieHandler>();

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();
builder.Services.AddScoped(x =>
(ICookieAuthenticationStateProvider)x.GetRequiredService<AuthenticationStateProvider>());

builder.Services.AddMudServices();

builder.Services.AddHttpClient();
builder.Services.AddHttpClient(Configuration.HttpClientName, opt =>
{
    opt.BaseAddress = backendUri;
}).AddHttpMessageHandler<CookieHandler>();

builder.Services.AddTransient<IAccountHandler, AccountHandler>();
builder.Services.AddTransient<IAgendamentoHandler, AgendamentoHandler>();
builder.Services.AddTransient<IAvaliacaoHandler, AvaliacaoHandler>();
builder.Services.AddTransient<ICorteHandler, CorteHandler>();
builder.Services.AddScoped<AdminNotificationClient>();
builder.Services.AddScoped<PushNotificationClient>();


await builder.Build().RunAsync();
