using BarberShop.Core.Responses.Account;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace BarberShop.Web.Pages.MeuPerfil
{
    public partial class EditPerfilPage : ComponentBase
    {

        #region Properties
        public bool IsBusy { get; set; }

        public EditProfileInputModel InputModel { get; set; } = new();

        #endregion

        #region Services

        [Inject]
        public IHttpClientFactory ClientFactory { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

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
                    NavigationManager.NavigateTo("/perfil");
                    return;
                }

                InputModel = new EditProfileInputModel
                {
                    Nome = user.Nome,
                    Email = user.Email,
                    Telefone = user.Telefone
                };
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
                NavigationManager.NavigateTo("/perfil");
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        #region Override

        protected override async Task OnInitializedAsync()
        {
            await LoadProfileAsync();
        }

        #endregion

        #region Methods

        public async Task OnValidSubmitAsync()
        {
            try
            {
                IsBusy = true;

                var response = await Client.PutAsJsonAsync("v1/identity/update-profile", InputModel);

                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Perfil atualizado com sucesso!", Severity.Success);
                    NavigationManager.NavigateTo("/perfil");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Snackbar.Add($"Erro ao atualizar perfil: {error}", Severity.Error);
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

        public class EditProfileInputModel
        {
            public string Nome { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Telefone { get; set; } = string.Empty;
        }
    }
}