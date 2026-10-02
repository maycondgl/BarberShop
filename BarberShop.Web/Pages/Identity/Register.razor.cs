using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Account;
using BarberShop.Web.Security;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Identity
{
    public partial class RegisterPage : ComponentBase
    {
        #region Properties

        public bool IsBusy { get; set; } = false;
        public bool ShowWelcomeModal { get; set; } = false;
        protected bool _showPassword = false;
        protected bool _showConfirmPassword = false;
        public RegisterRequest InputModel { get; set; } = new();
        public PatternMask PhoneMask = new PatternMask("(00)00000-0000")
        {
            CleanDelimiters = true
        };

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
                if (!string.IsNullOrWhiteSpace(InputModel.Telefone))
                {
                    InputModel.Telefone = System.Text.RegularExpressions.Regex.Replace(InputModel.Telefone, @"\D", "");
                }

                if (!string.IsNullOrWhiteSpace(InputModel.Email))
                {
                    InputModel.Email = InputModel.Email.Trim().ToLowerInvariant();
                }

                var result = await Handler.RegisterAsync(InputModel);

                if (result.IsSuccess)
                {
                    ShowWelcomeModal = true;
                    StateHasChanged();
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

        public void CloseWelcomeModal()
        {
            ShowWelcomeModal = false;
            NavigationManager.NavigateTo("/entrar");
        }

        #endregion
    }
}
