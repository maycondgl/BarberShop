using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Cortes;
using BarberShop.Core.Responses.Corte;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Net.Http.Json;
using System.Text.Json;

namespace BarberShop.Web.Pages.Admin.Cortes
{
    public partial class CreateCortePage : ComponentBase
    {
        #region Properties

        public bool IsBusy { get; set; }
        public CreateCorteRequest InputModel { get; set; } = new();

        #endregion

        #region Services
        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        public ICorteHandler CorteHandler { get; set; } = null!;

        [Inject]
        public HttpClient HttpClient { get; set; } = null!;

        #endregion

        #region Methods

        public async Task OnValidSubmitAsync()
        {
            try
            {
                IsBusy = true;

                var result = await CorteHandler.CreateAsync(InputModel);

                if (result.IsSuccess)
                {
                    Snackbar.Add("Corte criado com sucesso!", Severity.Success);
                    NavigationManager.NavigateTo("/admin/cortes");
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Erro ao criar corte", Severity.Error);
                }
            }catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task UploadImage(InputFileChangeEventArgs e)
        {
            Snackbar.Add("UploadImage foi chamado", Severity.Info);

            try
            {
                var file = e.File;

                using var content = new MultipartFormDataContent();

                var streamContent = new StreamContent(file.OpenReadStream(5_000_000));
                streamContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                content.Add(streamContent, "file", file.Name);

                var response = await HttpClient.PostAsync(
                    "http://localhost:5131/v1/cortes/upload-imagem",
                    content);

                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Snackbar.Add($"Erro ao enviar imagem: {json}", Severity.Error);
                    return;
                }

                var result = JsonSerializer.Deserialize<UploadCorteImagemResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (result is not null)
                {
                    InputModel.ImagemUrl = result.ImagemUrl;
                    Snackbar.Add("Imagem enviada com sucesso!", Severity.Success);
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
        #endregion
    }
}
