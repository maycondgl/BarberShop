using BarberShop.Api.Data;
using BarberShop.Api.Endpoints;
using BarberShop.Api.Handlers;
using BarberShop.Api.Models;
using BarberShop.Core.Handlers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BarberShop.Api.Models.Requests;

var builder = WebApplication.CreateBuilder(args);

var Connection = builder
    .Configuration
    .GetConnectionString("Connection") ?? string.Empty;

builder
    .Services
    .AddDbContext<BarberShopContext>(
    x =>
    {
        x.UseSqlServer(Connection);
    });
builder.Services
    .AddIdentityCore<User>()
    .AddRoles<IdentityRole<long>>()
    .AddEntityFrameworkStores<BarberShopContext>()
    .AddApiEndpoints();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( x =>
{
    x.CustomSchemaIds(n => n.FullName);
});
builder.Services.AddTransient<IAgendamentoHandler, AgendamentoHandler>();
builder.Services.AddTransient<IAvaliacaoHandler, AvaliacaoHandler>();
builder.Services.AddTransient<ICorteHandler, CorteHandler>();

builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
builder.Services.AddAuthorization();


var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => new { message = "OK" });
app.MapEndpoints();
app.MapGroup("v1/identity")
    .WithTags("Identity")
    .MapIdentityApi<User>();


app.MapGroup("v1/identity")
    .WithTags("Identity")
    .MapPost("/register-custom", async (
        UserManager<User> userManager,
        RegisterRequests request
    ) =>
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            NomeCompleto = request.NomeCompleto,
            PhoneNumber = request.NumeroTelefone
        };

        var result = await userManager.CreateAsync(user, request.Senha);

        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);

        return Results.Ok("Usuário criado com sucesso!");
    });

app.MapGroup("v1/identity")
    .WithTags("Identity")
    .MapPost("/logout", async (
        SignInManager<User> signInManager) =>
    {
        await signInManager.SignOutAsync();
        return Results.Ok();
    })
    .RequireAuthorization();

app.MapGroup("v1/identity")
    .WithTags("Identity")
    .MapPost("/roles", (
        ClaimsPrincipal user) =>
    {
        if (user.Identity is null || user.Identity.IsAuthenticated)
            return Results.Unauthorized();

        var identity = (ClaimsIdentity)user.Identity;
        var roles = identity
        .FindAll(identity.RoleClaimType)
        .Select(c => new
        {
            c.Issuer,
            c.OriginalIssuer,
            c.Type,
            c.Value,
            c.ValueType
        });

        return TypedResults.Json(roles);
    })
    .RequireAuthorization();

app.Run();