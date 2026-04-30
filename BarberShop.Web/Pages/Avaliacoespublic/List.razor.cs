using BarberShop.Core.Handlers;
using BarberShop.Core.Responses.Avaliacao;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Avaliacoes
{
    public class ListAvaliacaoPublicPage : ComponentBase
    {
        [Inject] public IAvaliacaoHandler Handler { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;

        public List<AvaliacaoResponse> Avaliacoespublic { get; set; } = [];
        public bool IsBusy { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            var result = await Handler.GetAllPublicAsync(1, 50);
            IsBusy = false;
            if (result.IsSuccess && result.Data is not null)
                Avaliacoespublic = result.Data;
            else
                Snackbar.Add(result.Message ?? "Erro ao carregar avaliações", Severity.Error);
        }
    }
}