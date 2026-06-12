using BarberShop.Api.Models;
using BarberShop.Core;
using BarberShop.Core.Requests.Account;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Account;
using Microsoft.AspNetCore.Identity;

namespace BarberShop.Api.Handlers;

public class AccountHandler
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole<long>> _roleManager;

    public AccountHandler(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<IdentityRole<long>> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
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
            var adminCreated = await TryPromoteFirstAdminAsync(user, request.ChaveAdmin);
            var message = adminCreated
                ? "Usuario criado como administrador!"
                : "Usuário criado com sucesso!";

            return new Response<string?>(message, 201, "Sucesso");
        }

        var errors = string.Join(", ", result.Errors.Select(x => x.Description));
        return new Response<string?>(null, 400, errors);
    }

    private async Task<bool> TryPromoteFirstAdminAsync(User user, string? adminKey)
    {
        const string adminRole = "Admin";

        if (string.IsNullOrWhiteSpace(Configuration.AdminSetupKey) ||
            string.IsNullOrWhiteSpace(adminKey) ||
            adminKey != Configuration.AdminSetupKey)
            return false;

        if (!await _roleManager.RoleExistsAsync(adminRole))
        {
            var createRoleResult = await _roleManager.CreateAsync(new IdentityRole<long>(adminRole));

            if (!createRoleResult.Succeeded)
                return false;
        }

        var admins = await _userManager.GetUsersInRoleAsync(adminRole);

        if (admins.Count > 0)
            return false;

        var addRoleResult = await _userManager.AddToRoleAsync(user, adminRole);
        return addRoleResult.Succeeded;
    }

    public async Task<Response<string?>> LoginAsync(LoginRequest request)
    {
        var result = await _signInManager.PasswordSignInAsync(request.Email, request.Senha, false, false);

        if (result.Succeeded)
            return new Response<string?>("Login realizado!", 200);

        return new Response<string?>(null, 401, "E-mail ou senha inválidos");
    }

    public async Task<Response<List<AdminUserResponse>>> GetUsersAsync()
    {
        const string adminRole = "Admin";
        var users = _userManager.Users
            .OrderBy(x => x.NomeCompleto)
            .ThenBy(x => x.Email)
            .ToList();

        var response = new List<AdminUserResponse>();

        foreach (var user in users)
        {
            response.Add(new AdminUserResponse
            {
                Id = user.Id,
                Nome = user.NomeCompleto,
                Email = user.Email ?? string.Empty,
                Telefone = user.PhoneNumber,
                IsAdmin = await _userManager.IsInRoleAsync(user, adminRole)
            });
        }

        return new Response<List<AdminUserResponse>>(response, 200);
    }

    public async Task<Response<string?>> AddAdminAsync(long userId)
    {
        const string adminRole = "Admin";

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return new Response<string?>(null, 404, "Usuário não encontrado");

        if (!await _roleManager.RoleExistsAsync(adminRole))
            await _roleManager.CreateAsync(new IdentityRole<long>(adminRole));

        if (await _userManager.IsInRoleAsync(user, adminRole))
            return new Response<string?>("Usuário já é administrador", 200);

        var result = await _userManager.AddToRoleAsync(user, adminRole);

        return result.Succeeded
            ? new Response<string?>("Usuário promovido a administrador", 200)
            : new Response<string?>(null, 400, string.Join(", ", result.Errors.Select(x => x.Description)));
    }

    public async Task<Response<string?>> RemoveAdminAsync(long userId)
    {
        const string adminRole = "Admin";

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return new Response<string?>(null, 404, "Usuário não encontrado");

        if (!await _userManager.IsInRoleAsync(user, adminRole))
            return new Response<string?>("Usuário não é administrador", 200);

        var admins = await _userManager.GetUsersInRoleAsync(adminRole);

        if (admins.Count <= 1)
            return new Response<string?>(null, 400, "Não é possível remover o último administrador");

        var result = await _userManager.RemoveFromRoleAsync(user, adminRole);

        return result.Succeeded
            ? new Response<string?>("Administrador removido", 200)
            : new Response<string?>(null, 400, string.Join(", ", result.Errors.Select(x => x.Description)));
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
