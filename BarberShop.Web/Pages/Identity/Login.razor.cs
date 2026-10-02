using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Account;
using BarberShop.Web.Security;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Identity
{
    public partial class LoginPage : ComponentBase
    {
        #region Properties

        public bool IsBusy { get; set; } = false;
        public LoginRequest InputModel { get; set; } = new();
        protected bool _showPassword = false;

        #endregion

        #region Services

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public IAccountHandler Handler { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        public ICookieAuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

        #endregion

        #region Override

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
                IsBusy = true;

                if (!string.IsNullOrWhiteSpace(InputModel.Email))
                {
                    InputModel.Email = InputModel.Email.Trim().ToLowerInvariant();
                }

                var result = await Handler.LoginAsync(InputModel);

                if (result.IsSuccess)
                {
                    await AuthenticationStateProvider.GetAuthenticationStateAsync();
                    AuthenticationStateProvider.NotifyAuthenticationStateChanged();
                    NavigationManager.NavigateTo("/");
                }
                else
                {
                    Snackbar.Add(result.Message, Severity.Error);
                }
            }
            catch (Exception ex)
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
