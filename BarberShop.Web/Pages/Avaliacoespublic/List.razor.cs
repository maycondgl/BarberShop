using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Avaliacao;
using BarberShop.Core.Responses.Avaliacao;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Avaliacoes
{
    public class ListAvaliacaoPublicPage : ComponentBase
    {
        [Inject] public IAvaliacaoHandler Handler { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;

        public List<AvaliacaoResponse> MinhasAvaliacoes { get; set; } = [];
        public bool IsBusy { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            await CarregarMinhasAvaliacoesAsync();
        }

        public async Task CarregarMinhasAvaliacoesAsync()
        {
            IsBusy = true;
            try
            {
                var request = new GetAllAvaliacaoRequest
                {
                    PageNumber = 1,
                    PageSize = 50
                };
                var result = await Handler.GetAllAsync(request);
                if (result.IsSuccess && result.Data is not null)
                {
                    MinhasAvaliacoes = result.Data;
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Erro ao carregar avaliações.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("Erro de comunicação: " + ex.Message, Severity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}