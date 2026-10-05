using BarberShop.Core.Handlers;
using BarberShop.Core.Responses.Barbeiro;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;

namespace BarberShop.Web.Pages.AreaBarbeiro
{
    public enum PeriodoTipo
    {
        Dia,
        Semana,
        Mes,
        Ano,
        Comparativo
    }

    public record YAxisTick(double Y, string Label);

    public record BarVisualItem(
        double X,
        double Y,
        double CenterX,
        double Width,
        double Height,
        string RotuloLinha1,
        string RotuloLinha2,
        string ValorFormatado);

    public class BarbeiroDashboardPage : ComponentBase
    {
        [Inject] public IBarbeiroHandler Handler { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;

        public BarbeiroDashboardResponse? DashboardData { get; set; }
        public bool IsBusy { get; set; } = true;
        public CultureInfo PtBr { get; } = new("pt-BR");
        public CultureInfo Inv => CultureInfo.InvariantCulture;

        public PeriodoTipo _periodoSelecionado = PeriodoTipo.Semana;
        public string _periodoDescricao = "Lucro detalhado dia a dia nos últimos 7 dias";
        public DateTime _dataReferencia = DateTime.Today;

        public List<BarVisualItem> _barItems = [];
        public List<YAxisTick> _yAxisTicks = [];

        public bool IsPeriodoAtual => _periodoSelecionado switch
        {
            PeriodoTipo.Dia => _dataReferencia.Date == DateTime.Today,
            PeriodoTipo.Semana => ObterInicioSemana(_dataReferencia) == ObterInicioSemana(DateTime.Today),
            PeriodoTipo.Mes => _dataReferencia.Year == DateTime.Today.Year && _dataReferencia.Month == DateTime.Today.Month,
            PeriodoTipo.Ano => _dataReferencia.Year == DateTime.Today.Year,
            _ => _dataReferencia.Date == DateTime.Today
        };

        public string PeriodoFormatado
        {
            get
            {
                switch (_periodoSelecionado)
                {
                    case PeriodoTipo.Dia:
                        var diaStr = _dataReferencia.ToString("dddd, dd/MM/yyyy", PtBr);
                        return char.ToUpper(diaStr[0]) + diaStr[1..];
                    case PeriodoTipo.Semana:
                        var seg = ObterInicioSemana(_dataReferencia);
                        var dom = seg.AddDays(6);
                        return $"Semana de {seg:dd/MM} a {dom:dd/MM/yyyy}";
                    case PeriodoTipo.Mes:
                        var mesStr = _dataReferencia.ToString("MMMM 'de' yyyy", PtBr);
                        return char.ToUpper(mesStr[0]) + mesStr[1..];
                    case PeriodoTipo.Ano:
                        return $"Ano de {_dataReferencia.Year}";
                    case PeriodoTipo.Comparativo:
                    default:
                        return $"Referência: {_dataReferencia:dd/MM/yyyy}";
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
            _dataReferencia = _periodoSelecionado switch
            {
                PeriodoTipo.Dia => _dataReferencia.AddDays(direcao),
                PeriodoTipo.Semana => _dataReferencia.AddDays(direcao * 7),
                PeriodoTipo.Mes => _dataReferencia.AddMonths(direcao),
                PeriodoTipo.Ano => _dataReferencia.AddYears(direcao),
                _ => _dataReferencia.AddMonths(direcao)
            };
            CalcularGraficoBarras();
            StateHasChanged();
        }

        public void IrParaHoje()
        {
            _dataReferencia = DateTime.Today;
            CalcularGraficoBarras();
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            await CarregarDashboardAsync();
        }

        public async Task CarregarDashboardAsync()
        {
            IsBusy = true;
            try
            {
                var result = await Handler.GetDashboardAsync();
                if (result.IsSuccess && result.Data is not null)
                {
                    DashboardData = result.Data;
                    CalcularGraficoBarras();
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Falha ao carregar dashboard do barbeiro.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("Erro ao comunicar com o servidor: " + ex.Message, Severity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void MudarPeriodo(PeriodoTipo periodo)
        {
            _periodoSelecionado = periodo;
            CalcularGraficoBarras();
            StateHasChanged();
        }

        public void CalcularGraficoBarras()
        {
            _barItems.Clear();
            _yAxisTicks.Clear();

            if (DashboardData is null) return;

            var agsValidos = (DashboardData.TodosAgendamentos.Any()
                ? DashboardData.TodosAgendamentos
                : DashboardData.AgendamentosMes.Concat(DashboardData.AgendamentosSemana).Concat(DashboardData.AgendamentosHoje))
                .Where(a => a.Status != "Cancelado")
                .GroupBy(a => a.Id)
                .Select(g => g.First())
                .ToList();

            var dadosPeriodo = new List<(string l1, string l2, decimal valor)>();

            switch (_periodoSelecionado)
            {
                case PeriodoTipo.Dia:
                    var diaAlvo = _dataReferencia.Date;
                    var agsDia = agsValidos.Where(a => a.Data.Date == diaAlvo).ToList();
                    _periodoDescricao = $"Ganhos em {diaAlvo:dd/MM/yyyy} distribuídos por turno de trabalho";
                    dadosPeriodo.Add(("Manhã", "08h-12h", agsDia.Where(a => a.Data.Hour < 12).Sum(a => a.Valor)));
                    dadosPeriodo.Add(("Tarde", "12h-17h", agsDia.Where(a => a.Data.Hour >= 12 && a.Data.Hour < 17).Sum(a => a.Valor)));
                    dadosPeriodo.Add(("Noite", "17h-19h", agsDia.Where(a => a.Data.Hour >= 17).Sum(a => a.Valor)));
                    break;

                case PeriodoTipo.Semana:
                    var seg = ObterInicioSemana(_dataReferencia);
                    _periodoDescricao = $"Lucro dia a dia na semana de {seg:dd/MM} a {seg.AddDays(6):dd/MM/yyyy}";
                    for (int i = 0; i < 7; i++)
                    {
                        var dia = seg.AddDays(i);
                        var agsDoDia = agsValidos.Where(a => a.Data.Date == dia).ToList();
                        var diaNomeRaw = dia.ToString("ddd", PtBr).TrimEnd('.');
                        var diaNome = char.ToUpper(diaNomeRaw[0]) + diaNomeRaw[1..];
                        dadosPeriodo.Add((diaNome, dia.ToString("dd/MM"), agsDoDia.Sum(a => a.Valor)));
                    }
                    break;

                case PeriodoTipo.Mes:
                    var anoAlvoMes = _dataReferencia.Year;
                    var mesAlvo = _dataReferencia.Month;
                    var totalDiasMes = DateTime.DaysInMonth(anoAlvoMes, mesAlvo);
                    var mesNome = _dataReferencia.ToString("MMMM", PtBr);
                    mesNome = char.ToUpper(mesNome[0]) + mesNome[1..];
                    _periodoDescricao = $"Lucro de {mesNome} de {anoAlvoMes} consolidado por semanas";

                    var semanasDef = new (int ini, int fim, string rotulo1, string rotulo2)[]
                    {
                        (1, 7, "Sem 1", "01 a 07"),
                        (8, 14, "Sem 2", "08 a 14"),
                        (15, 21, "Sem 3", "15 a 21"),
                        (22, totalDiasMes, "Sem 4", $"22 a {totalDiasMes:D2}")
                    };

                    foreach (var (ini, fim, r1, r2) in semanasDef)
                    {
                        var inicioData = new DateTime(anoAlvoMes, mesAlvo, ini);
                        var fimData = new DateTime(anoAlvoMes, mesAlvo, fim, 23, 59, 59);
                        var agsSem = agsValidos.Where(a => a.Data >= inicioData && a.Data <= fimData).ToList();
                        dadosPeriodo.Add((r1, r2, agsSem.Sum(a => a.Valor)));
                    }
                    break;

                case PeriodoTipo.Ano:
                    var anoAlvo = _dataReferencia.Year;
                    _periodoDescricao = $"Faturamento mês a mês no ano de {anoAlvo}";
                    for (int m = 1; m <= 12; m++)
                    {
                        var mesNomeRaw = new DateTime(anoAlvo, m, 1).ToString("MMM", PtBr).TrimEnd('.');
                        var mesAbrev = char.ToUpper(mesNomeRaw[0]) + mesNomeRaw[1..];
                        var agsMes = agsValidos.Where(a => a.Data.Year == anoAlvo && a.Data.Month == m).ToList();
                        dadosPeriodo.Add((mesAbrev, anoAlvo.ToString(), agsMes.Sum(a => a.Valor)));
                    }
                    break;

                case PeriodoTipo.Comparativo:
                default:
                    var diaComp = _dataReferencia.Date;
                    var segComp = ObterInicioSemana(_dataReferencia);
                    var domComp = segComp.AddDays(6).AddDays(1).AddTicks(-1);
                    var mesIni = new DateTime(_dataReferencia.Year, _dataReferencia.Month, 1);
                    var mesFim = mesIni.AddMonths(1).AddTicks(-1);
                    var anoIni = new DateTime(_dataReferencia.Year, 1, 1);
                    var anoFim = anoIni.AddYears(1).AddTicks(-1);

                    var vDia = agsValidos.Where(a => a.Data.Date == diaComp).Sum(a => a.Valor);
                    var vSem = agsValidos.Where(a => a.Data >= segComp && a.Data <= domComp).Sum(a => a.Valor);
                    var vMes = agsValidos.Where(a => a.Data >= mesIni && a.Data <= mesFim).Sum(a => a.Valor);
                    var vAno = agsValidos.Where(a => a.Data >= anoIni && a.Data <= anoFim).Sum(a => a.Valor);

                    _periodoDescricao = $"Comparativo consolidado em {_dataReferencia:dd/MM/yyyy}: Dia vs Semana vs Mês vs Ano";
                    dadosPeriodo.Add(("Dia", _dataReferencia.ToString("dd/MM"), vDia));
                    dadosPeriodo.Add(("Semana", $"{segComp:dd/MM}-{segComp.AddDays(6):dd/MM}", vSem));
                    dadosPeriodo.Add(("Mês", _dataReferencia.ToString("MMM/yy", PtBr), vMes));
                    dadosPeriodo.Add(("Ano", _dataReferencia.Year.ToString(), vAno));
                    break;
            }

            const double plotLeft = 65.0;
            const double plotRight = 605.0;
            const double plotWidth = plotRight - plotLeft; // 540
            const double plotTop = 25.0;
            const double plotBaseline = 295.0;
            const double plotHeight = plotBaseline - plotTop; // 270

            var maxValorReal = dadosPeriodo.Any() ? (double)dadosPeriodo.Max(x => x.valor) : 0.0;
            double maxTeto;

            if (maxValorReal <= 0)
            {
                maxTeto = 100.0;
            }
            else if (maxValorReal <= 50)
            {
                maxTeto = 50.0;
            }
            else if (maxValorReal <= 100)
            {
                maxTeto = 100.0;
            }
            else if (maxValorReal <= 250)
            {
                maxTeto = 250.0;
            }
            else if (maxValorReal <= 500)
            {
                maxTeto = 500.0;
            }
            else
            {
                maxTeto = Math.Ceiling(maxValorReal / 100.0) * 100.0;
            }

            // Criar 5 marcas no eixo Y
            for (int i = 0; i <= 4; i++)
            {
                var prop = (double)i / 4.0;
                var valorTick = maxTeto * prop;
                var yPos = plotBaseline - (prop * plotHeight);
                _yAxisTicks.Add(new YAxisTick(yPos, valorTick.ToString("C0", PtBr)));
            }

            var n = dadosPeriodo.Count;
            if (n == 0) return;

            var slotWidth = plotWidth / n;
            var barWidth = n switch
            {
                <= 3 => 72.0,
                4 => 62.0,
                <= 7 => 42.0,
                _ => 26.0
            };

            for (int i = 0; i < n; i++)
            {
                var item = dadosPeriodo[i];
                var v = (double)item.valor;
                var centerX = plotLeft + ((i + 0.5) * slotWidth);
                var barX = centerX - (barWidth / 2.0);

                var h = v > 0 ? (v / maxTeto) * plotHeight : 0.0;
                h = Math.Min(h, plotHeight);
                var barY = plotBaseline - h;

                _barItems.Add(new BarVisualItem(
                    X: barX,
                    Y: barY,
                    CenterX: centerX,
                    Width: barWidth,
                    Height: h,
                    RotuloLinha1: item.l1,
                    RotuloLinha2: item.l2,
                    ValorFormatado: item.valor.ToString("C0", PtBr)
                ));
            }
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
