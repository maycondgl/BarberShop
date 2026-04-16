using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Account;
using BarberShop.Web.Security;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Identity
{
    public partial class RegisterPage : ComponentBase
    {
        #region Dependencies

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public IAccountHandler Handler { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        public ICookieAuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

        #endregion

        #region Properties

        public bool IsBusy { get; set; } = false;
        public RegisterRequest InputModel { get; set; } = new();

        #endregion

        #region Overrides

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity is { IsAuthenticated: true })
                NavigationManager.NavigateTo("/");
        }

        #endregion

        #region Methods
        public async Task OnValidSubmitAsync()
        {
            try
            {
                var result = await Handler.RegisterAsync(InputModel);

                if (result.IsSuccess)
                {
                    Snackbar.Add(result.Message, Severity.Success);
                    NavigationManager.NavigateTo("/entrar");
                }
                else
                    Snackbar.Add(result.Message, Severity.Error);
            }
            catch(Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion
    }
}