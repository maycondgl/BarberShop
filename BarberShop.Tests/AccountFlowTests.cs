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
    [InlineData("11987654321")]
    [InlineData("21987654321")]
    [InlineData("27987654321")]
    [InlineData("31987654321")]
    [InlineData("41987654321")]
    [InlineData("48987654321")]
    [InlineData("51987654321")]
    [InlineData("61987654321")]
    [InlineData("62987654321")]
    [InlineData("71987654321")]
    [InlineData("79987654321")]
    [InlineData("81987654321")]
    [InlineData("85987654321")]
    [InlineData("8899702863")]
    [InlineData("88999702863")]
    [InlineData("91987654321")]
    [InlineData("92987654321")]
    public void RegisterRequest_AcceptsValidBrazilianDdd(string phone)
    {
        var request = ValidRegisterRequest();
        request.Telefone = phone;

        Assert.Empty(Validate(request));
    }

    [Theory]
    [InlineData("859999999")]
    [InlineData("859999999999")]
    [InlineData("8599999999A")]
    [InlineData("(85)999999999")]
    [InlineData("00999998888")]
    [InlineData("01999998888")]
    [InlineData("20999998888")]
    [InlineData("23999998888")]
    [InlineData("25999998888")]
    [InlineData("26999998888")]
    [InlineData("29999998888")]
    [InlineData("30999998888")]
    [InlineData("36999998888")]
    [InlineData("39999998888")]
    [InlineData("50999998888")]
    [InlineData("52999998888")]
    [InlineData("70999998888")]
    [InlineData("72999998888")]
    [InlineData("76999998888")]
    [InlineData("78999998888")]
    public void RegisterRequest_RejectsInvalidPhone(string phone)
    {
        var request = ValidRegisterRequest();
        request.Telefone = phone;

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.Telefone)));
    }

    [Theory]
    [InlineData("11987654321")]
    [InlineData("85987654321")]
    [InlineData("8899702863")]
    public void UpdateProfileRequest_AcceptsValidBrazilianDdd(string phone)
    {
        var request = new UpdateProfileRequest
        {
            Nome = "Cliente Atualizado",
            Email = "cliente@barbershop.com",
            Telefone = phone
        };

        Assert.Empty(Validate(request));
    }

    [Theory]
    [InlineData("23999998888")]
    [InlineData("00999998888")]
    [InlineData("859999999")]
    public void UpdateProfileRequest_RejectsInvalidPhone(string phone)
    {
        var request = new UpdateProfileRequest
        {
            Nome = "Cliente Atualizado",
            Email = "cliente@barbershop.com",
            Telefone = phone
        };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.Telefone)));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("abcdef")]
    [InlineData("Abcdef")]
    [InlineData("Abcde1")]
    [InlineData("ABCDEF!")]
    [InlineData("abcdef!")]
    [InlineData("Abcdef!")]
    [InlineData("Senha!")]
    public void RegisterRequest_RejectsPasswordMissingComplexityRequirements(string password)
    {
        var request = ValidRegisterRequest();
        request.Senha = password;
        request.ConfirmarSenha = password;

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.Senha)));
    }

    [Theory]
    [InlineData("cliente")]
    [InlineData("cliente@")]
    [InlineData("cliente@barbershop")]
    [InlineData("@barbershop.com")]
    [InlineData("cliente barbershop.com")]
    [InlineData("cliente@gma.com")]
    [InlineData("cliente@hotm.com")]
    [InlineData("cliente@outloo.com")]
    [InlineData("cliente@fg.com")]
    [InlineData("cliente@ab.com")]
    [InlineData("cliente@.com")]
    [InlineData("cliente@qualquercoisa")]
    [InlineData("cliente@site.xyz")]
    [InlineData("cliente@empresa.com.br")]
    public void RegisterRequest_RejectsInvalidEmailFormat(string email)
    {
        var request = ValidRegisterRequest();
        request.Email = email;

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.Email)));
    }

    [Theory]
    [InlineData("cliente@gmail.com")]
    [InlineData("cliente@outlook.com")]
    [InlineData("cliente@outlook.com.br")]
    [InlineData("cliente@hotmail.com")]
    [InlineData("cliente@yahoo.com")]
    [InlineData("cliente@yahoo.com.br")]
    [InlineData("cliente@icloud.com")]
    [InlineData("cliente@live.com")]
    [InlineData("cliente@uol.com.br")]
    [InlineData("cliente@bol.com.br")]
    [InlineData("cliente@barbershop.com")]
    [InlineData("CLIENTE@GMAIL.COM")]
    public void RegisterRequest_AcceptsValidEmailDomains(string email)
    {
        var request = ValidRegisterRequest();
        request.Email = email;

        var errors = Validate(request);

        Assert.DoesNotContain(errors, error => error.MemberNames.Contains(nameof(request.Email)));
    }

    [Fact]
    public void RegisterRequest_RejectsMismatchedConfirmarSenha()
    {
        var request = ValidRegisterRequest();
        request.ConfirmarSenha = "OutraSenhaDiferente123!";

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.ConfirmarSenha)));
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
            Senha = "Senha123!",
            ConfirmarSenha = "Senha123!"
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
