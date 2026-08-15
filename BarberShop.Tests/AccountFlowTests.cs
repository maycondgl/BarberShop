using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using BarberShop.Api;
using BarberShop.Core.Requests.Account;
using BarberShop.Web.Handlers;
using Microsoft.Extensions.Configuration;

namespace BarberShop.Tests;

public class AccountFlowTests
{
    [Fact]
    public void RegisterRequest_AcceptsExactlyElevenDigits()
    {
        var request = ValidRegisterRequest();

        Assert.Empty(Validate(request));
    }

    [Theory]
    [InlineData("8599999999")]
    [InlineData("859999999999")]
    [InlineData("8599999999A")]
    [InlineData("(85)999999999")]
    public void RegisterRequest_RejectsPhoneThatIsNotElevenDigits(string phone)
    {
        var request = ValidRegisterRequest();
        request.Telefone = phone;

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.Telefone)));
    }

    [Fact]
    public void DatabaseConnectionSettings_UsesDefaultConnectionAsFallback()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Connection"] = string.Empty,
                ["ConnectionStrings:DefaultConnection"] = " Server=sql;Database=BarberShop; "
            })
            .Build();

        var result = DatabaseConnectionSettings.Resolve(configuration);

        Assert.Equal("Server=sql;Database=BarberShop;", result);
    }

    [Fact]
    public void DatabaseConnectionSettings_RejectsMissingConnection()
    {
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(
            () => DatabaseConnectionSettings.Resolve(configuration));

        Assert.Contains("ConnectionStrings__Connection", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_PreservesApiProblemMessage()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent(
                "{\"title\":\"Serviço de autenticação indisponível\",\"detail\":\"Banco temporariamente indisponível.\"}",
                Encoding.UTF8,
                "application/json")
        });
        var handler = new AccountHandler(new TestHttpClientFactory(client));

        var response = await handler.LoginAsync(new LoginRequest
        {
            Email = "cliente@barbershop.com",
            Senha = "Senha123!"
        });

        Assert.False(response.IsSuccess);
        Assert.Equal("Banco temporariamente indisponível.", response.Message);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsValidationMessageFromApi()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(
                "{\"errors\":{\"Telefone\":[\"O telefone deve conter 11 dígitos\"]}}",
                Encoding.UTF8,
                "application/json")
        });
        var handler = new AccountHandler(new TestHttpClientFactory(client));

        var response = await handler.RegisterAsync(ValidRegisterRequest());

        Assert.False(response.IsSuccess);
        Assert.Equal("O telefone deve conter 11 dígitos", response.Message);
    }

    private static RegisterRequest ValidRegisterRequest()
        => new()
        {
            Nome = "Cliente BarberShop",
            Telefone = "85999999999",
            Email = "cliente@barbershop.com",
            Senha = "Senha123!"
        };

    private static List<ValidationResult> Validate(object request)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), errors, true);
        return errors;
    }

    private static HttpClient CreateClient(
        Func<HttpRequestMessage, HttpResponseMessage> responder)
        => new(new TestMessageHandler(responder))
        {
            BaseAddress = new Uri("https://localhost/")
        };

    private sealed class TestHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class TestMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }
}
