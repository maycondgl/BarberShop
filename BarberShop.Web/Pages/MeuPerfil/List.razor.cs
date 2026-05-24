using BarberShop.Core.Handlers;
using BarberShop.Core.Models.Account;
using BarberShop.Core.Responses.Account;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace BarberShop.Web.Pages.MeuPerfil
{
    public partial class ListPerfilPage : ComponentBase
    {
        #region Properties

        public bool IsBusy { get; set; }
        public bool IsEditing { get; set; }
        public ProfileInputModel InputModel { get; set; } = new();

        #endregion

        #region Services

        [Inject]
        public IHttpClientFactory ClientFactory { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public IDialogService DialogService { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        #endregion

        #region Override

        protected override async Task OnInitializedAsync()
        {
            await LoadProfileAsync();
        }

        #endregion

        #region Private

        private HttpClient Client =>
            ClientFactory.CreateClient(Configuration.HttpClientName);

        private async Task LoadProfileAsync()
        {
            try
            {
                IsBusy = true;

                var user = await Client.GetFromJsonAsync<ProfileResponse>("v1/identity/profile");

                if (user is null)
                {
                    Snackbar.Add("Não foi possível carregar os dados do perfil.", Severity.Error);
                    return;
                }

                InputModel = new ProfileInputModel
                {
                    Nome = user?.Nome ?? string.Empty,
                    Email = user?.Email ?? string.Empty,
                    Telefone = user?.Telefone ?? string.Empty
                };
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

        #region Methods

        public void OnEditClicked()
        {
            IsEditing = true;
        }

        public async Task OnDeleteClickedAsync()
        {
            var response = await Client.DeleteAsync("v1/identity/delete-profile");

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Conta excluída com sucesso.", Severity.Success);
                NavigationManager.NavigateTo("/login", true);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add(error, Severity.Error);
            }
        }
        public async Task OnSaveClickedAsync()
        {
            try { 
            IsBusy = true;

            var response = await Client.PutAsJsonAsync("v1/identity/update-profile", InputModel);

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Perfil atualizado com sucesso!", Severity.Success);
                IsEditing = false;
            }
            else
            {
                Snackbar.Add("Não foi possível atualizar o perfil.", Severity.Error);
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
        public class ProfileInputModel
        {
            public string Nome { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Telefone { get; set; } = string.Empty;
        }
    }

        #endregion
    }