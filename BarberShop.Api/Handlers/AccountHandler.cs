using BarberShop.Api.Models;
using BarberShop.Core.Requests.Account;
using BarberShop.Core.Responses;
using Microsoft.AspNetCore.Identity;

namespace BarberShop.Api.Handlers;

public class AccountHandler
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AccountHandler(
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Response<string?>> RegisterAsync(RegisterRequest request)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            NomeCompleto = request.Nome,
            PhoneNumber = request.Telefone,
            Ativo = true
        };

        var result = await _userManager.CreateAsync(user, request.Senha); 

        if (result.Succeeded)
        {
            return new Response<string?>("Usuário criado com sucesso!", 201, "Sucesso");
        }

        var errors = string.Join(", ", result.Errors.Select(x => x.Description));
        return new Response<string?>(null, 400, errors);
    }

    public async Task<Response<string?>> LoginAsync(LoginRequest request)
    {
        var result = await _signInManager.PasswordSignInAsync(request.Email, request.Senha, false, false);

        if (result.Succeeded)
            return new Response<string?>("Login realizado!", 200);

        return new Response<string?>(null, 401, "E-mail ou senha inválidos");
    }

    public async Task<Response<string?>> LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        return new Response<string?>("Logout realizado com sucesso!", 200);
    }

    public async Task<Response<string?>> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return new Response<string?>(null, 400, "Usuário não encontrado");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return new Response<string?>(token, 200, "Token gerado com sucesso");
    }

    public async Task<Response<string?>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null) return new Response<string?>(null, 400, "Usuário não encontrado");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (result.Succeeded)
            return new Response<string?>("Senha alterada com sucesso!", 200);

        return new Response<string?>(null, 400, "Erro ao resetar senha");
    }
}