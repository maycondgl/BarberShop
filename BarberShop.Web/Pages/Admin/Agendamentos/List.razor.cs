using BarberShop.Core.Enums;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Responses.Agendamento;
using BarberShop.Web.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;

namespace BarberShop.Web.Pages.Admin.Agendamentos
{
    public class ListAdminAgendamentoPage : ComponentBase
    {
        #region Properties

        public bool IsBusy { get; set; } = false;
        public List<AgendamentoResponse> TodosAgendamentos { get; set; } = [];
        public List<AgendamentoResponse> Agendamentos { get; set; } = [];

        public string FiltroPeriodo { get; set; } = "Mes";
        public DateTime _dataReferencia { get; set; } = DateTime.Today;
        public CultureInfo PtBr { get; } = new("pt-BR");

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
        public int TotalPeriodo { get; set; }
        public int TotalPendentes { get; set; }
        public int TotalAceitos { get; set; }
        public int TotalConcluidos { get; set; }
        public decimal FaturamentoPeriodo { get; set; }

        public int PendingPage { get; set; } = 1;
        public int PageSize { get; set; } = 6;

        public bool IsPeriodoAtual => FiltroPeriodo switch
        {
            "Dia" => _dataReferencia.Date == DateTime.Today,
            "Semana" => ObterInicioSemana(_dataReferencia) == ObterInicioSemana(DateTime.Today),
            "Mes" => _dataReferencia.Year == DateTime.Today.Year && _dataReferencia.Month == DateTime.Today.Month,
            "Ano" => _dataReferencia.Year == DateTime.Today.Year,
            _ => true
        };

        public string PeriodoFormatado
        {
            get
            {
                switch (FiltroPeriodo)
                {
                    case "Dia":
                        var diaStr = _dataReferencia.ToString("dddd, dd/MM/yyyy", PtBr);
                        return char.ToUpper(diaStr[0]) + diaStr[1..];
                    case "Semana":
                        var seg = ObterInicioSemana(_dataReferencia);
                        var dom = seg.AddDays(6);
                        return $"Semana de {seg:dd/MM} a {dom:dd/MM/yyyy}";
                    case "Mes":
                        var mesStr = _dataReferencia.ToString("MMMM 'de' yyyy", PtBr);
                        return char.ToUpper(mesStr[0]) + mesStr[1..];
                    case "Ano":
                        return $"Ano de {_dataReferencia.Year}";
                    case "Todos":
                    default:
                        return "Histórico Geral Completo";
                }
            }
        }

        public static DateTime ObterInicioSemana(DateTime d)
        {
            var diff = ((int)d.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return d.Date.AddDays(-diff);
        }

        public List<AgendamentoResponse> FilteredAgendamentos =>
            Agendamentos
                .Where(Filter)
                .Where(x => string.IsNullOrWhiteSpace(StatusFilter) ||
                            x.Status.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase) ||
                            (StatusFilter == "Concluido" && IsConcluido(x.Status)))
                .ToList();

        public List<AgendamentoResponse> PagedAgendamentos =>
            FilteredAgendamentos
                .Skip((PendingPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

        public int PendingTotalPages => Math.Max(1, (int)Math.Ceiling(FilteredAgendamentos.Count / (double)PageSize));

        #endregion

        #region Services

        [Inject] 
        public IAgendamentoHandler Handler { get; set; } = null!;

        [Inject] 
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public IDialogService DialogService { get; set; } = null!;

        [Inject]
        public PushNotificationClient PushNotificationClient { get; set; } = null!;

        #endregion

        #region Override

        protected override async Task OnInitializedAsync()
        {
            await LoadAdminAgendamentosAsync();
        }

        #endregion

        #region Methods

        public void NavegarPeriodo(int direcao)
        {
            _dataReferencia = FiltroPeriodo switch
            {
                "Dia" => _dataReferencia.AddDays(direcao),
                "Semana" => _dataReferencia.AddDays(direcao * 7),
                "Mes" => _dataReferencia.AddMonths(direcao),
                "Ano" => _dataReferencia.AddYears(direcao),
                _ => _dataReferencia.AddMonths(direcao)
            };
            AtualizarFiltro();
            StateHasChanged();
        }

        public void IrParaHoje()
        {
            _dataReferencia = DateTime.Today;
            AtualizarFiltro();
            StateHasChanged();
        }

        public void SetPeriodo(string periodo)
        {
            FiltroPeriodo = periodo;
            AtualizarFiltro();
            StateHasChanged();
        }

        public void SetStatusFilter(string status)
        {
            StatusFilter = status;
            PendingPage = 1;
        }

        public void PreviousPendingPage()
            => PendingPage = Math.Max(1, PendingPage - 1);

        public void NextPendingPage()
            => PendingPage = Math.Min(PendingTotalPages, PendingPage + 1);

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
            if (status.Equals("Pendente", StringComparison.OrdinalIgnoreCase))
                return "admin-status admin-status--pending";

            if (status.Equals("Aceito", StringComparison.OrdinalIgnoreCase))
                return "admin-status admin-status--accepted";

            if (IsConcluido(status))
                return "admin-status admin-status--done";

            if (status.Equals("Cancelado", StringComparison.OrdinalIgnoreCase))
                return "admin-status admin-status--canceled";

            return "admin-status";
        }

        public Func<AgendamentoResponse, bool> Filter => x =>
            string.IsNullOrWhiteSpace(SearchTerm) ||
            x.NomeCliente.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
            x.Status.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
            x.CorteTitulo.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
            x.FilialNome.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
            x.BarbeiroNome.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase);

        private void AtualizarFiltro()
        {
            PendingPage = 1;

            IEnumerable<AgendamentoResponse> baseList = FiltroPeriodo switch
            {
                "Dia" => TodosAgendamentos.Where(x => x.Data.Date == _dataReferencia.Date).OrderBy(x => x.Data),
                "Semana" => TodosAgendamentos.Where(x =>
                {
                    var seg = ObterInicioSemana(_dataReferencia);
                    var dom = seg.AddDays(7).AddTicks(-1);
                    return x.Data >= seg && x.Data <= dom;
                }).OrderByDescending(x => x.Data),
                "Mes" => TodosAgendamentos.Where(x => x.Data.Year == _dataReferencia.Year && x.Data.Month == _dataReferencia.Month).OrderByDescending(x => x.Data),
                "Ano" => TodosAgendamentos.Where(x => x.Data.Year == _dataReferencia.Year).OrderByDescending(x => x.Data),
                "Todos" => TodosAgendamentos.OrderByDescending(x => x.Data),
                _ => TodosAgendamentos.OrderByDescending(x => x.Data)
            };

            var lista = baseList.ToList();
            Agendamentos = lista;

            TotalPeriodo = lista.Count;
            TotalPendentes = lista.Count(x => x.Status.Equals("Pendente", StringComparison.OrdinalIgnoreCase));
            TotalAceitos = lista.Count(x => x.Status.Equals("Aceito", StringComparison.OrdinalIgnoreCase));
            TotalConcluidos = lista.Count(x => IsConcluido(x.Status));
            FaturamentoPeriodo = lista.Where(x => IsConcluido(x.Status)).Sum(x => x.Valor);
        }

        public int ObterContagemPeriodo(string periodo)
        {
            return periodo switch
            {
                "Dia" => TodosAgendamentos.Count(a => a.Data.Date == _dataReferencia.Date),
                "Semana" => TodosAgendamentos.Count(a =>
                {
                    var seg = ObterInicioSemana(_dataReferencia);
                    var dom = seg.AddDays(7).AddTicks(-1);
                    return a.Data >= seg && a.Data <= dom;
                }),
                "Mes" => TodosAgendamentos.Count(a => a.Data.Year == _dataReferencia.Year && a.Data.Month == _dataReferencia.Month),
                "Ano" => TodosAgendamentos.Count(a => a.Data.Year == _dataReferencia.Year),
                "Todos" => TodosAgendamentos.Count,
                _ => TodosAgendamentos.Count
            };
        }

        public async Task LoadAdminAgendamentosAsync()
        {
            IsBusy = true;
            try
            {
                var request = new GetAllAgendamentoRequest
                {
                    UserId = 0,
                    PageNumber = 1,
                    PageSize = 1000
                };

                var result = await Handler.GetAllAdminAsync(request);
                if (result.IsSuccess && result.Data is not null)
                {
                    TodosAgendamentos = result.Data
                        .OrderByDescending(x => x.Data)
                        .ToList();

                    AtualizarFiltro();
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Erro ao carregar agendamentos", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro ao carregar agendamentos: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsBusy = false;
            }
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
                Snackbar.Add("Agendamento aceito com sucesso!", Severity.Success);
                await LoadAdminAgendamentosAsync();
            }
            else
                Snackbar.Add(result.Message ?? "Erro ao aceitar agendamento.", Severity.Error);
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
                Snackbar.Add("Agendamento concluído com sucesso!", Severity.Success);
                await LoadAdminAgendamentosAsync();
            }
            else
                Snackbar.Add(result.Message ?? "Erro ao concluir agendamento.", Severity.Error);
        }

        public async Task OnDeleteClickedAsync(long id)
        {
            var result = await Handler.DeleteAsync(id);
            if (result.IsSuccess)
            {
                Snackbar.Add("Agendamento excluído!", Severity.Success);
                TodosAgendamentos.RemoveAll(x => x.Id == id);
                AtualizarFiltro();
            }
            else
                Snackbar.Add(result.Message ?? "Erro ao excluir agendamento.", Severity.Error);
        }

        public async Task OnAtivarNotificacoesClickedAsync()
        {
            var result = await PushNotificationClient.SubscribeAdminAsync();

            Snackbar.Add(
                result.Message,
                result.Success ? Severity.Success : Severity.Warning);
        }

        public static bool IsConcluido(string status)
            => status.Equals("Concluido", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("Concluído", StringComparison.OrdinalIgnoreCase);

        #endregion
    }
}
