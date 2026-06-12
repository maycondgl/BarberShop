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
    }
}
