using BarberShop.Api.common.Api;
using BarberShop.Api.Models;
using BarberShop.Core.Requests.Account;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Endpoints.Accounts;

public class LoginEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/login", HandleAsync)
              .AllowAnonymous();

    private static async Task<IResult> HandleAsync(
        [FromQuery] bool? useCookies,
        [FromQuery] bool? useSessionCookies,
        [FromBody] LoginRequest body,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<LoginEndpoint> logger)
    {
        var validationErrors = Validate(body);
        if (validationErrors.Count > 0)
            return Results.ValidationProblem(validationErrors);

        var email = body.Email.Trim();

        try
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user is null || !user.Ativo)
                return Results.Unauthorized();

            var passwordOk = await signInManager.CheckPasswordSignInAsync(
                user,
                body.Senha,
                lockoutOnFailure: false);

            if (!passwordOk.Succeeded)
                return Results.Unauthorized();

            var isPersistent = useSessionCookies != true;

            await signInManager.SignInAsync(
                user,
                isPersistent: isPersistent);

            return Results.Ok(new
            {
                message = "Login realizado com sucesso",
                user.Id,
                user.UserName,
                user.Email,
                user.PhoneNumber
            });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Falha ao acessar o banco durante o login de {Email}", email);
            return Results.Problem(
                title: "Serviço de autenticação indisponível",
                detail: "Não foi possível acessar o banco de dados. Tente novamente em instantes.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static Dictionary<string, string[]> Validate(LoginRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            results,
            validateAllProperties: true);

        return results
            .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty)
                .Select(member => new { Member = member, Message = result.ErrorMessage ?? "Valor inválido" }))
            .GroupBy(result => result.Member)
            .ToDictionary(
                group => group.Key,
                group => group.Select(result => result.Message).Distinct().ToArray());
    }
}
