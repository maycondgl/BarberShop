using BarberShop.Core.Requests.Account;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace BarberShop.Web.Pages.MeuPerfil;

public partial class AlterarSenhaPage : ComponentBase
{
    public ChangePasswordRequest InputModel { get; set; } = new();

    [Inject]
    public IHttpClientFactory ClientFactory { get; set; } = null!;

    [Inject]
    public ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    private HttpClient Client =>
        ClientFactory.CreateClient(Configuration.HttpClientName);

    public async Task OnSubmitAsync()
    {
        try
        {
            var response = await Client.PutAsJsonAsync(
                "v1/identity/change-password",
                InputModel);

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Senha alterada com sucesso!", Severity.Success);

                NavigationManager.NavigateTo("/perfil");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                Snackbar.Add(error, Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }
}