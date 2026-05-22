using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Cortes;
using BarberShop.Core.Responses.Corte;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Text.Json;

namespace BarberShop.Web.Pages.Admin.Cortes
{
    public partial class UpdatePageCorte : ComponentBase
    {
        #region Properties

        public bool IsBusy { get; set; }
        public UpdateCorteRequest InputModel { get; set; } = new();
        [Parameter]
        public long Id { get; set; }

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

        #region Override

        protected override async Task OnInitializedAsync()
        {
            await LoadCorteAsync();
        }

        #endregion

        #region Methods
        private async Task LoadCorteAsync()
        {
            try
            {
                IsBusy = true;

                var result = await CorteHandler.GetByIdAsync(new GetCorteByIdRequest
                {
                    Id = Id
                });

                if (result.IsSuccess && result.Data is not null)
                {
                    InputModel = new UpdateCorteRequest
                    {
                        Id = result.Data.Id,
                        Titulo = result.Data.Titulo,
                        Preco = result.Data.Preco,
                        DuracaoMinutos = result.Data.DuracaoMinutos,
                        Descricao = result.Data.Descricao,
                        ImagemUrl = result.Data.ImagemUrl,
                        Ativo = result.Data.Ativo
                    };
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Não foi possível carregar o corte.", Severity.Error);
                    NavigationManager.NavigateTo("/admin/cortes");
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
                NavigationManager.NavigateTo("/admin/cortes");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task OnValidSubmitAsync()
        {
            try
            {
                IsBusy = true;

                var result = await CorteHandler.UpdateAsync(InputModel);

                if (result.IsSuccess)
                {
                    Snackbar.Add("Corte atualizado com sucesso!", Severity.Success);
                    NavigationManager.NavigateTo("/admin/cortes");
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Erro ao atualizar o corte.", Severity.Error);
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