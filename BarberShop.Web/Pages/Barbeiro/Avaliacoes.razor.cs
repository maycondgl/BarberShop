using BarberShop.Core.Handlers;
using BarberShop.Core.Responses.Avaliacao;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.AreaBarbeiro
{
    public class BarbeiroAvaliacoesPage : ComponentBase
    {
        [Inject] public IBarbeiroHandler BarbeiroHandler { get; set; } = null!;
        [Inject] public IAvaliacaoHandler AvaliacaoHandler { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;

        public List<AvaliacaoResponse> Avaliacoes { get; set; } = [];
        public double MediaEstrelas { get; set; } = 5.0;
        public int TotalAvaliacoes { get; set; }
        public bool IsBusy { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            await CarregarAvaliacoesAsync();
        }

        public async Task CarregarAvaliacoesAsync()
        {
            IsBusy = true;
            try
            {
                var dashResult = await BarbeiroHandler.GetDashboardAsync();
                if (dashResult.IsSuccess && dashResult.Data is not null)
                {
                    Avaliacoes = dashResult.Data.MinhasAvaliacoes;
                    MediaEstrelas = dashResult.Data.MediaAvaliacoes;
                    TotalAvaliacoes = dashResult.Data.TotalAvaliacoes;
                }
                else
                {
                    Snackbar.Add(dashResult.Message ?? "Falha ao carregar avaliações.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("Erro ao carregar avaliações: " + ex.Message, Severity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
