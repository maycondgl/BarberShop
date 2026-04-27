using BarberShop.Core.Enums;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Requests.Avaliacao;
using BarberShop.Web.Handlers;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Avaliacoes
{
    public partial class ListAvaliacaoPage : ComponentBase
    {
        #region Properties

        public bool IsBusy { get; set; } = false;
        public List<Agendamento> Agendamentos { get; set; } = new();
        public List<long> AgendamentosAvaliados { get; set; } = new();

        #endregion

        #region Services

        [Inject]
        public IAvaliacaoHandler Handler { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public IAgendamentoHandler AgendamentoHandler { get; set; } = null!;

        #endregion
        

        protected override async Task OnInitializedAsync()
        {
            var agendamentosResult = await AgendamentoHandler.GetAllAsync(
                new GetAllAgendamentoRequest { PageNumber = 1, PageSize = 100 });

            if (agendamentosResult.IsSuccess)
                Agendamentos = agendamentosResult.Data?
                    .Where(x => x.Status == EStatusAgendamento.Concluido)
                    .ToList() ?? new();

            var avaliacoesResult = await Handler.GetAllAsync(
                new GetAllAvaliacaoRequest { PageNumber = 1, PageSize = 100 });

            if (avaliacoesResult.IsSuccess)
                AgendamentosAvaliados = avaliacoesResult.Data?
                    .Select(x => x.AgendamentoId)
                    .ToList() ?? new();
        }
    }
}
