using BarberShop.Api.Handlers;
using BarberShop.Core.Requests.Account;
using BarberShop.Api.common.Api;
using System.ComponentModel.DataAnnotations;

namespace BarberShop.Api.Endpoints.Accounts;

public class RegisterEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/register", HandleAsync)
              .AllowAnonymous();

    private static async Task<IResult> HandleAsync(
        RegisterRequest request,
        AccountHandler handler,
        ILogger<RegisterEndpoint> logger)
    {
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
            return Results.ValidationProblem(validationErrors);

        request.Nome = request.Nome.Trim();
        request.Telefone = request.Telefone.Trim();
        request.Email = request.Email.Trim();

        try
        {
            var result = await handler.RegisterAsync(request);
            return result.IsSuccess
                ? Results.Created("", result)
                : Results.BadRequest(result);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Falha ao acessar o banco durante o cadastro de {Email}", request.Email);
            return Results.Problem(
                title: "Serviço de cadastro indisponível",
                detail: "Não foi possível acessar o banco de dados. Tente novamente em instantes.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static Dictionary<string, string[]> Validate(RegisterRequest request)
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
