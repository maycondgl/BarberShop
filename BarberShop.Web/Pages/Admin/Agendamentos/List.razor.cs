using BarberShop.Core.Enums;
using BarberShop.Core.Extensions;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Responses.Agendamento;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Admin.Agendamentos
{
    public class ListAdminAgendamentoPage : ComponentBase
    {
        #region Properties

        public bool IsBusy { get; set; } = false;
        public List<AgendamentoResponse> TodosAgendamentos { get; set; } = [];
        public List<AgendamentoResponse> Agendamentos { get; set; } = [];
        public List<AgendamentoResponse> AgendamentosHistorico { get; set; } = [];
        public string SearchTerm { get; set; } = string.Empty;
        public int SemanaRange { get; set; } = 1;
        public List<int> SemanaRangeOptions { get; } = [1, 2, 3];

        #endregion

        #region Services

        [Inject] 
        public IAgendamentoHandler Handler { get; set; } = null!;

        [Inject] 
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public IDialogService DialogService { get; set; } = null!;

        #endregion

        #region Private Methods

        private const int DefaultPageSize = 500;

      

        #endregion

        #region Override

        protected override async Task OnInitializedAsync()
        {
            await LoadAdminAgendamentosAsync();
        }

        #endregion

        #region Methods

        public Func<AgendamentoResponse, bool> Filter => x =>
            string.IsNullOrWhiteSpace(SearchTerm) ||
            x.NomeCliente.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
            x.Status.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase);

        public async Task OnPesquisarPeriodoClickedAsync()
            => LoadHistoricoByWeeks();

        private async Task LoadAdminAgendamentosAsync()
        {
            var request = new GetAllAgendamentoRequest
            {
                UserId = 0,
                PageNumber = 1,
                PageSize = DefaultPageSize
            };

            var result = await Handler.GetAllAdminAsync(request);
            if (result.IsSuccess && result.Data is not null)
            {
                TodosAgendamentos = result.Data
                    .OrderByDescending(x => x.Data)
                    .ToList();
                Agendamentos = TodosAgendamentos
                    .Where(x => x.Status != "Concluido")
                    .ToList();
            }
            else
            {
                Snackbar.Add(result.Message ?? "Erro ao carregar", Severity.Error);
            }
        }

        private void LoadHistoricoByWeeks()
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-(SemanaRange * 7) + 1);

            AgendamentosHistorico = TodosAgendamentos
                .Where(x => x.Data.Date >= startDate && x.Data.Date <= endDate)
                .OrderByDescending(x => x.Data)
                .ToList();

            Snackbar.Add(
                $"Histórico de {startDate:dd/MM/yyyy} até {endDate:dd/MM/yyyy}",
                Severity.Info);
        }

        public async Task OnAceitarClickedAsync(long id)
        {
            var request = new UpdateStatusAgendamentoRequest 
            { 
                Id = id, 
                Status = EStatusAgendamento.Aceito 
            };

            var result = await Handler.UpdateStatusAsync(request);
            if (result.IsSuccess)
            {
                Snackbar.Add("Agendamento aceito!", Severity.Success);
                await OnInitializedAsync();
            }
            else
                Snackbar.Add(result.Message ?? "Erro", Severity.Error);
        }

        public async Task OnConcluirClickedAsync(long id)
        {
            var request = new UpdateStatusAgendamentoRequest 
            { 
                Id = id, 
                Status = EStatusAgendamento.Concluido 
            };

            var result = await Handler.UpdateStatusAsync(request);
            if (result.IsSuccess)
            {
                Snackbar.Add("Agendamento concluído!", Severity.Success);
                Agendamentos.RemoveAll(x => x.Id == id);
                TodosAgendamentos.RemoveAll(x => x.Id == id);
                AgendamentosHistorico.RemoveAll(x => x.Id == id);
                StateHasChanged();
            }
            else
                Snackbar.Add(result.Message ?? "Erro", Severity.Error);
        }


        public async Task OnDeleteClickedAsync(long id)
        {
            var result = await Handler.DeleteAsync(id);
            if (result.IsSuccess)
            {
                Snackbar.Add("Agendamento excluído!", Severity.Success);
                Agendamentos.RemoveAll(x => x.Id == id);
            }
            else
                Snackbar.Add(result.Message ?? "Erro", Severity.Error);
        }
    }

        #endregion
}