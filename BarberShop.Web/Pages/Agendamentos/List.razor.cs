using BarberShop.Core.Enums;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Requests.Avaliacao;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Agendamentos
{
    public partial class ListAgendamentoPage : ComponentBase
    {
        #region Properties

        public bool IsBusy { get; set; } = false;
        public List<Agendamento> Agendamentos { get; set; } = [];
        public string SearchTerm { get; set; } = string.Empty;
        public List<long> AgendamentosAvaliados { get; set; } = new();

        public Agendamento? ProximoAgendamento => Agendamentos
            .Where(x => x.Status != EStatusAgendamento.Cancelado && x.Status != EStatusAgendamento.Concluido)
            .OrderBy(x => x.Data)
            .FirstOrDefault();

        public List<Agendamento> HistoricoAgendamentos => Agendamentos
            .Where(x => ProximoAgendamento == null || x.Id != ProximoAgendamento.Id)
            .OrderByDescending(x => x.Data)
            .ToList();

        public bool MostrarHistorico { get; set; } = false;

        public void AlternarHistorico()
        {
            MostrarHistorico = !MostrarHistorico;
        }

        #endregion

        #region Services

        [Inject]
        public IAvaliacaoHandler AvaliacaoHandler { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public IAgendamentoHandler Handler { get; set; } = null!;

        [Inject]
        public IDialogService DialogService { get; set; } = null!;

        #endregion

        #region Overrides

        protected override async Task OnInitializedAsync()
        {
            await CarregarAgendamentosAsync();
        }

        public async Task CarregarAgendamentosAsync()
        {
            IsBusy = true;
            try
            {
                var request = new GetAllAgendamentoRequest
                {
                    PageNumber = 1,
                    PageSize = 50
                };
                var result = await Handler.GetAllAsync(request);
                if (result.IsSuccess)
                    Agendamentos = result.Data ?? new List<Agendamento>();
                else
                    Snackbar.Add(result.Message ?? "Erro ao carregar agendamentos", Severity.Error);

                var avaliacoes = await AvaliacaoHandler.GetAllAsync(
                    new GetAllAvaliacaoRequest { PageNumber = 1, PageSize = 100 });

                if (avaliacoes.IsSuccess)
                {
                    AgendamentosAvaliados = avaliacoes.Data?
                        .Select(x => x.AgendamentoId)
                        .ToList() ?? new();
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("Erro ao carregar agendamentos: " + ex.Message, Severity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task OnDeleteClickedAsync(long id)
        {
            bool? confirm = await DialogService.ShowMessageBox(
                "Cancelar / Excluir Agendamento",
                "Tem certeza que deseja cancelar e excluir este agendamento?",
                yesText: "Sim, Cancelar", cancelText: "Voltar");

            if (confirm != true)
                return;

            var result = await Handler.DeleteAsync(id);

            if (result.IsSuccess)
            {
                Agendamentos.RemoveAll(x => x.Id == id);
                Snackbar.Add("Agendamento cancelado com sucesso!", Severity.Success);
                StateHasChanged();
            }
            else
            {
                Snackbar.Add(result.Message ?? "Erro ao remover agendamento", Severity.Error);
            }
        }

        #endregion

        #region Methods

        public Func<Agendamento, bool> Filter => agendamento =>
        {
            if (string.IsNullOrEmpty(SearchTerm))
                return true;

            if (agendamento.Id.ToString().Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                return true;

            if (agendamento.Corte != null && agendamento.Corte.Titulo.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                return true;

            if (agendamento.Filial != null && agendamento.Filial.Nome.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                return true;

            if (agendamento.Barbeiro != null && agendamento.Barbeiro.Nome.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                return true;

            if (agendamento.Data.ToString("dd/MM/yyyy HH:mm").Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                return true;

            if (agendamento.Status.ToString().Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        #endregion
    }
}
