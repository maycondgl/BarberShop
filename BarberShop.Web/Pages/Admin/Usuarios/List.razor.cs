using BarberShop.Core.Handlers;
using BarberShop.Core.Responses.Account;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Admin.Usuarios
{
    public partial class ListPageUsuarios : ComponentBase
    {
        public bool IsBusy { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public List<AdminUserResponse> Usuarios { get; set; } = [];

        public int AdminCount => Usuarios.Count(x => x.IsAdmin);

        public IEnumerable<AdminUserResponse> FilteredUsers
            => string.IsNullOrWhiteSpace(SearchTerm)
                ? Usuarios
                : Usuarios.Where(x =>
                    x.Nome.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    x.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (x.Telefone?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

        [Inject]
        public IAccountHandler AccountHandler { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            await LoadUsersAsync();
        }

        public async Task LoadUsersAsync()
        {
            try
            {
                IsBusy = true;

                var result = await AccountHandler.GetUsersAsync();

                if (result.IsSuccess && result.Data is not null)
                {
                    Usuarios = result.Data;
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Erro ao carregar usuários", Severity.Error);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task AddAdminAsync(long userId)
        {
            var result = await AccountHandler.AddAdminAsync(userId);

            if (result.IsSuccess)
            {
                Snackbar.Add(result.Message ?? "Usuário promovido a administrador", Severity.Success);
                await LoadUsersAsync();
            }
            else
            {
                Snackbar.Add(result.Message ?? "Erro ao promover usuário", Severity.Error);
            }
        }

        public async Task RemoveAdminAsync(long userId)
        {
            var result = await AccountHandler.RemoveAdminAsync(userId);

            if (result.IsSuccess)
            {
                Snackbar.Add(result.Message ?? "Administrador removido", Severity.Success);
                await LoadUsersAsync();
            }
            else
            {
                Snackbar.Add(result.Message ?? "Erro ao remover administrador", Severity.Error);
            }
        }

        public static string GetInitials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "U";

            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                0 => "U",
                1 => parts[0].Length >= 2 ? parts[0][..2].ToUpperInvariant() : parts[0][..1].ToUpperInvariant(),
                _ => $"{parts[0][..1]}{parts[^1][..1]}".ToUpperInvariant()
            };
        }

        public static string FormatPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return "Não informado";

            var digits = System.Text.RegularExpressions.Regex.Replace(phone, @"\D", "");
            return digits.Length switch
            {
                11 => $"({digits[..2]}) {digits.Substring(2, 5)}-{digits.Substring(7, 4)}",
                10 => $"({digits[..2]}) {digits.Substring(2, 4)}-{digits.Substring(6, 4)}",
                _ => phone
            };
        }
    }
}
