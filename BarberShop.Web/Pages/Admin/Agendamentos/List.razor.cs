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
        public List<AgendamentoResponse> Pendentes { get; set; } = [];
        public List<AgendamentoResponse> Agendamentos { get; set; } = [];
        public List<AgendamentoResponse> AgendamentosHistorico { get; set; } = [];

        private string _searchTerm = string.Empty;

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                PendingPage = 1;
            }
        }

        public string StatusFilter { get; set; } = string.Empty;
        public int TotalHoje { get; set; }
        public int TotalPendentes { get; set; }
        public int TotalMes { get; set; }
        public int TotalConcluidosMes { get; set; }
        public decimal FaturamentoMes { get; set; }
        public DateTime HistoricoStartDate { get; set; } = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        public DateTime HistoricoEndDate { get; set; } = DateTime.Today;
        public int PendingPage { get; set; } = 1;
        public int HistoryPage { get; set; } = 1;
        public int PageSize { get; set; } = 4;

        public List<AgendamentoResponse> FilteredAgendamentos =>
            Agendamentos
                .Where(Filter)
                .Where(x => string.IsNullOrWhiteSpace(StatusFilter) ||
                            x.Status.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();

        public List<AgendamentoResponse> PagedAgendamentos =>
            FilteredAgendamentos
                .Skip((PendingPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

        public List<AgendamentoResponse> PagedHistorico =>
            AgendamentosHistorico
                .Skip((HistoryPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

        public int PendingTotalPages => Math.Max(1, (int)Math.Ceiling(FilteredAgendamentos.Count / (double)PageSize));
        public int HistoryTotalPages => Math.Max(1, (int)Math.Ceiling(AgendamentosHistorico.Count / (double)PageSize));
        public string HistoricoPeriodoLabel => $"{HistoricoStartDate:dd/MM/yyyy} até {HistoricoEndDate:dd/MM/yyyy}";

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
            x.Status.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
            x.CorteTitulo.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase);

        public void SetStatusFilter(string status)
        {
            StatusFilter = status;
            PendingPage = 1;
        }

        public void PreviousPendingPage()
            => PendingPage = Math.Max(1, PendingPage - 1);

        public void NextPendingPage()
            => PendingPage = Math.Min(PendingTotalPages, PendingPage + 1);

        public void PreviousHistoryPage()
            => HistoryPage = Math.Max(1, HistoryPage - 1);

        public void NextHistoryPage()
            => HistoryPage = Math.Min(HistoryTotalPages, HistoryPage + 1);

        public string ResultLabel(int count)
            => count == 1 ? "resultado" : "resultados";

        public string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "?";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 1
                ? parts[0][..1].ToUpperInvariant()
                : $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
        }

        public string GetStatusText(string status)
            => IsConcluido(status) ? "Concluído" : status;

        public string GetStatusClass(string status)
        {
            if (status == "Pendente")
                return "admin-status admin-status--pending";

            if (status == "Aceito")
                return "admin-status admin-status--accepted";

            if (IsConcluido(status))
                return "admin-status admin-status--done";

            return "admin-status";
        }

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

                var hoje = DateTime.Today;
                var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
                var inicioProximoMes = inicioMes.AddMonths(1);

                Agendamentos = TodosAgendamentos
                    .Where(x => !IsConcluido(x.Status) &&
                                x.Data.Date >= inicioMes &&
                                x.Data.Date < inicioProximoMes)
                    .ToList();

                TotalHoje = TodosAgendamentos
                    .Count(x => x.Data.Date == hoje);

                TotalPendentes = TodosAgendamentos
                    .Count(x => x.Status == "Pendente" &&
                                x.Data.Date >= inicioMes &&
                                x.Data.Date < inicioProximoMes);

                TotalMes = TodosAgendamentos
                    .Count(x => x.Data.Date >= inicioMes && x.Data.Date < inicioProximoMes);

                TotalConcluidosMes = TodosAgendamentos
                    .Count(x => IsConcluido(x.Status) &&
                                x.Data.Date >= inicioMes &&
                                x.Data.Date < inicioProximoMes);

                FaturamentoMes = TodosAgendamentos
                    .Where(x => IsConcluido(x.Status) &&
                                x.Data.Date >= inicioMes &&
                                x.Data.Date < inicioProximoMes)
                    .Sum(x => x.Valor);

                LoadHistoricoDoMes();
            }
            else
            {
                Snackbar.Add(result.Message ?? "Erro ao carregar", Severity.Error);
            }
        }

        private void LoadHistoricoDoMes()
        {
            var hoje = DateTime.Today;
            HistoricoStartDate = new DateTime(hoje.Year, hoje.Month, 1);
            HistoricoEndDate = HistoricoStartDate.AddMonths(1).AddDays(-1);
            HistoryPage = 1;

            AgendamentosHistorico = TodosAgendamentos
                .Where(x => IsConcluido(x.Status) &&
                            x.Data.Date >= HistoricoStartDate &&
                            x.Data.Date <= HistoricoEndDate)
                .OrderByDescending(x => x.Data)
                .ToList();
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
                await LoadAdminAgendamentosAsync();
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

        public static bool IsConcluido(string status)
            => status.Equals("Concluido", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("Concluído", StringComparison.OrdinalIgnoreCase);

    }

        #endregion
}
