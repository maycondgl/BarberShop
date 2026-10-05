using BarberShop.Core.Handlers;
using BarberShop.Core.Responses.Agendamento;
using BarberShop.Core.Responses.Barbeiro;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;

namespace BarberShop.Web.Pages.AreaBarbeiro
{
    public class BarbeiroAgendamentosPage : ComponentBase
    {
        [Inject] public IBarbeiroHandler Handler { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;

        public BarbeiroDashboardResponse? DashboardData { get; set; }
        public bool IsBusy { get; set; } = true;
        public string FiltroPeriodo { get; set; } = "Dia";
        public DateTime _dataReferencia { get; set; } = DateTime.Today;
        public string BuscaTexto { get; set; } = string.Empty;
        public CultureInfo PtBr { get; } = new("pt-BR");

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
            StateHasChanged();
        }

        public void IrParaHoje()
        {
            _dataReferencia = DateTime.Today;
            StateHasChanged();
        }

        public void SetPeriodo(string periodo)
        {
            FiltroPeriodo = periodo;
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            await CarregarDadosAsync();
        }

        public async Task CarregarDadosAsync()
        {
            IsBusy = true;
            try
            {
                var result = await Handler.GetDashboardAsync();
                if (result.IsSuccess && result.Data is not null)
                {
                    DashboardData = result.Data;
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Falha ao carregar agendamentos.", Severity.Error);
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

        private List<AgendamentoResponse> ObterTodosAgendamentosBase()
        {
            if (DashboardData is null) return [];

            if (DashboardData.TodosAgendamentos.Any())
                return DashboardData.TodosAgendamentos;

            return DashboardData.AgendamentosMes
                .Concat(DashboardData.AgendamentosSemana)
                .Concat(DashboardData.AgendamentosHoje)
                .GroupBy(a => a.Id)
                .Select(g => g.First())
                .ToList();
        }

        public int ObterContagemPeriodo(string periodo)
        {
            var todos = ObterTodosAgendamentosBase();
            return periodo switch
            {
                "Dia" => todos.Count(a => a.Data.Date == _dataReferencia.Date),
                "Semana" => todos.Count(a =>
                {
                    var seg = ObterInicioSemana(_dataReferencia);
                    var dom = seg.AddDays(7).AddTicks(-1);
                    return a.Data >= seg && a.Data <= dom;
                }),
                "Mes" => todos.Count(a => a.Data.Year == _dataReferencia.Year && a.Data.Month == _dataReferencia.Month),
                "Ano" => todos.Count(a => a.Data.Year == _dataReferencia.Year),
                "Todos" => todos.Count,
                _ => todos.Count
            };
        }

        public List<AgendamentoResponse> ObterAgendamentosFiltrados()
        {
            var todos = ObterTodosAgendamentosBase();

            IEnumerable<AgendamentoResponse> filtrados = FiltroPeriodo switch
            {
                "Dia" => todos.Where(a => a.Data.Date == _dataReferencia.Date).OrderBy(a => a.Data),
                "Semana" => todos.Where(a =>
                {
                    var seg = ObterInicioSemana(_dataReferencia);
                    var dom = seg.AddDays(7).AddTicks(-1);
                    return a.Data >= seg && a.Data <= dom;
                }).OrderByDescending(a => a.Data),
                "Mes" => todos.Where(a => a.Data.Year == _dataReferencia.Year && a.Data.Month == _dataReferencia.Month).OrderByDescending(a => a.Data),
                "Ano" => todos.Where(a => a.Data.Year == _dataReferencia.Year).OrderByDescending(a => a.Data),
                "Todos" => todos.OrderByDescending(a => a.Data),
                _ => todos.OrderByDescending(a => a.Data)
            };

            if (string.IsNullOrWhiteSpace(BuscaTexto))
                return filtrados.ToList();

            var termo = BuscaTexto.Trim().ToLowerInvariant();
            return filtrados.Where(a =>
                (a.NomeCliente?.ToLowerInvariant().Contains(termo) ?? false) ||
                (a.CorteTitulo?.ToLowerInvariant().Contains(termo) ?? false) ||
                (a.Status?.ToLowerInvariant().Contains(termo) ?? false)
            ).ToList();
        }

        public static string GetStatusColor(string status) => status switch
        {
            "Concluido" or "Concluído" => "background: rgba(76, 175, 80, 0.2); color: #4CAF50; border: 1px solid #4CAF50;",
            "Aceito" => "background: rgba(33, 150, 243, 0.2); color: #2196F3; border: 1px solid #2196F3;",
            "Cancelado" => "background: rgba(244, 67, 54, 0.2); color: #F44336; border: 1px solid #F44336;",
            _ => "background: rgba(255, 193, 7, 0.2); color: #FFC107; border: 1px solid #FFC107;"
        };

        public static string GetStatusLabel(string status) => status switch
        {
            "Concluido" or "Concluído" => "Concluído",
            "Aceito" => "Confirmado",
            "Cancelado" => "Cancelado",
            _ => "Pendente"
        };
    }
}
