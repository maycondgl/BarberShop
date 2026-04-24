using BarberShop.Core.Enums;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Agendamentos;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Avaliacoes
{
    public partial class IndexAvaliacaoPage : ComponentBase
    {
        #region Properties

        public bool IsBusy { get; set; } = false;

        public List<Agendamento> AgendamentosParaAvaliar { get; set; } = new();

        #endregion

        #region Services

        [Inject]
        public IAgendamentoHandler AgendamentoHandler { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        #endregion

        #region Methods

        protected override async Task OnInitializedAsync()
        {
            IsBusy = true;
            try
            {
                var result = await AgendamentoHandler.GetAllAsync(new GetAllAgendamentoRequest
                {
                    PageNumber = 1,
                    PageSize = 25
                });

                if (result.IsSuccess && result.Data != null)
                {
                    var hoje = DateTime.Today;
                    var amanha = hoje.AddDays(1);

                    AgendamentosParaAvaliar = result.Data
                        .Where(x => x.Status == EStatusAgendamento.Concluido &&
                                    x.Data >= hoje && x.Data < amanha)
                        .ToList();

                    if (AgendamentosParaAvaliar.Any())
                    {
                        NavigationManager.NavigateTo($"/avaliacoes/adicionar/{AgendamentosParaAvaliar.First().Id}");
                    }
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Não foi possível carregar seus agendamentos.", Severity.Error);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion
    }
}