using System.Globalization;
using BarberShop.Core.Handlers;
using BarberShop.Core.Responses.Dashboard;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Admin.Dashboard
{
    public partial class DashboardPage : ComponentBase
    {
        #region Models

        public enum PeriodoTipo
        {
            Dia,
            Semana,
            Mes,
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

        public class PieSliceVisual
        {
            public string Titulo { get; set; } = string.Empty;
            public double Porcentagem { get; set; }
            public int Quantidade { get; set; }
            public string Cor { get; set; } = string.Empty;
            public string PathData { get; set; } = string.Empty;
            public bool IsFullCircle { get; set; }

            public double LineX1 { get; set; }
            public double LineY1 { get; set; }
            public double LineX2 { get; set; }
            public double LineY2 { get; set; }
            public double LineX3 { get; set; }
            public double LineY3 { get; set; }

            public double TextX { get; set; }
            public double TextY { get; set; }
            public string TextAnchor { get; set; } = "start";
            public string RotuloPorcentagem { get; set; } = string.Empty;
            public bool IsRightSide { get; set; }
            public double AngleMid { get; set; }
        }

        #endregion

        #region Properties

        protected static readonly CultureInfo PtBr = new("pt-BR");
        protected static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        protected static readonly string[] CoresGrafico = new[]
        {
            "#2979FF", // Azul
            "#00E676", // Verde
            "#FFD600", // Amarelo
            "#FF9100", // Laranja
            "#FF1744", // Vermelho
            "#D500F9", // Roxo
            "#00E5FF", // Ciano
            "#76FF03", // Lima
            "#FF4081"  // Rosa
        };

        protected bool _isBusy;
        protected DashboardResponse? _dashboardData;
        protected PeriodoTipo _periodoSelecionado = PeriodoTipo.Comparativo;
        protected string _periodoDescricao = "Comparativo geral: Hoje vs Esta Semana vs Este Mês";

        protected List<YAxisTick> _yAxisTicks = new();
        protected List<BarVisualItem> _barItems = new();
        protected List<PieSliceVisual> _pieSlices = new();

        #endregion

        #region Services

        [Inject]
        public IDashboardHandler DashboardHandler { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        #endregion

        #region Override

        protected override async Task OnInitializedAsync()
        {
            await CarregarDashboardAsync();
        }

        #endregion

        #region Methods

        protected async Task CarregarDashboardAsync()
        {
            try
            {
                _isBusy = true;
                var result = await DashboardHandler.GetDashboardAsync();

                if (result.IsSuccess && result.Data != null)
                {
                    _dashboardData = result.Data;

                    CalcularGraficoBarras();
                    CalcularFatiasPieChart();
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Não foi possível carregar as métricas do dashboard", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro ao carregar dashboard: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isBusy = false;
            }
        }

        protected void MudarPeriodo(PeriodoTipo tipo)
        {
            _periodoSelecionado = tipo;
            CalcularGraficoBarras();
            StateHasChanged();
        }

        protected void CalcularGraficoBarras()
        {
            _barItems.Clear();
            _yAxisTicks.Clear();

            if (_dashboardData == null) return;

            var dadosPeriodo = new List<(string l1, string l2, decimal valor)>();

            switch (_periodoSelecionado)
            {
                case PeriodoTipo.Dia:
                    _periodoDescricao = "Lucro de hoje detalhado por turno (Manhã, Tarde e Noite)";
                    dadosPeriodo.Add(("Manhã", "08h-12h", _dashboardData.LucroPorTurnoHoje.ElementAtOrDefault(0)?.Valor ?? 0));
                    dadosPeriodo.Add(("Tarde", "12h-17h", _dashboardData.LucroPorTurnoHoje.ElementAtOrDefault(1)?.Valor ?? 0));
                    dadosPeriodo.Add(("Noite", "17h-19h", _dashboardData.LucroPorTurnoHoje.ElementAtOrDefault(2)?.Valor ?? 0));
                    break;

                case PeriodoTipo.Semana:
                    _periodoDescricao = "Lucro detalhado dia a dia nos últimos 7 dias";
                    var hoje = DateTime.Today;
                    for (int i = 6; i >= 0; i--)
                    {
                        var dia = hoje.AddDays(-i);
                        var item = _dashboardData.LucroPorDia.ElementAtOrDefault(6 - i);
                        var diaNomeRaw = dia.ToString("ddd", PtBr).TrimEnd('.');
                        var diaNome = char.ToUpper(diaNomeRaw[0]) + diaNomeRaw[1..];
                        dadosPeriodo.Add((diaNome, dia.ToString("dd/MM"), item?.Valor ?? 0));
                    }
                    break;

                case PeriodoTipo.Mes:
                    _periodoDescricao = "Lucro do mês vigente consolidado por semanas";
                    dadosPeriodo.Add(("Sem 1", "01 a 07", _dashboardData.LucroPorSemanaMes.ElementAtOrDefault(0)?.Valor ?? 0));
                    dadosPeriodo.Add(("Sem 2", "08 a 14", _dashboardData.LucroPorSemanaMes.ElementAtOrDefault(1)?.Valor ?? 0));
                    dadosPeriodo.Add(("Sem 3", "15 a 21", _dashboardData.LucroPorSemanaMes.ElementAtOrDefault(2)?.Valor ?? 0));
                    dadosPeriodo.Add(("Sem 4", "22 a fim", _dashboardData.LucroPorSemanaMes.ElementAtOrDefault(3)?.Valor ?? 0));
                    break;

                case PeriodoTipo.Comparativo:
                default:
                    _periodoDescricao = "Comparativo consolidado: Hoje vs Esta Semana vs Este Mês";
                    dadosPeriodo.Add(("Hoje", "", _dashboardData.LucroDia));
                    dadosPeriodo.Add(("Semana", "", _dashboardData.LucroSemana));
                    dadosPeriodo.Add(("Mês", "", _dashboardData.LucroMes));
                    break;
            }

            const double plotLeft = 65.0;
            const double plotRight = 495.0;
            const double plotWidth = plotRight - plotLeft; // 430
            const double plotTop = 30.0;
            const double plotBaseline = 235.0;
            const double plotHeight = plotBaseline - plotTop; // 205

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
                <= 3 => 44.0,
                4 => 36.0,
                _ => 24.0
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
                    ValorFormatado: item.valor.ToString("C", PtBr)
                ));
            }
        }

        protected void CalcularFatiasPieChart()
        {
            _pieSlices.Clear();

            if (_dashboardData == null) return;

            var cortesComAgendamento = _dashboardData.CortesMetricas
                .Where(c => c.Quantidade > 0)
                .OrderByDescending(c => c.Quantidade)
                .ToList();

            if (!cortesComAgendamento.Any()) return;

            const double cx = 210.0;
            const double cy = 135.0;
            const double r = 75.0;

            var totalQuantidade = cortesComAgendamento.Sum(c => c.Quantidade);
            if (totalQuantidade <= 0) return;

            if (cortesComAgendamento.Count == 1)
            {
                var unico = cortesComAgendamento[0];
                _pieSlices.Add(new PieSliceVisual
                {
                    Titulo = unico.Titulo,
                    Quantidade = unico.Quantidade,
                    Porcentagem = 100.0,
                    Cor = CoresGrafico[0],
                    IsFullCircle = true,
                    LineX1 = cx + r,
                    LineY1 = cy,
                    LineX2 = cx + r + 20,
                    LineY2 = cy,
                    LineX3 = cx + r + 45,
                    LineY3 = cy,
                    TextX = cx + r + 50,
                    TextY = cy + 4,
                    TextAnchor = "start",
                    RotuloPorcentagem = "100,0%"
                });
                return;
            }

            double anguloAtual = -Math.PI / 2; // Inicia às 12 horas

            for (int i = 0; i < cortesComAgendamento.Count; i++)
            {
                var item = cortesComAgendamento[i];
                var cor = CoresGrafico[i % CoresGrafico.Length];
                var proporcao = (double)item.Quantidade / totalQuantidade;
                var deltaAngulo = proporcao * 2 * Math.PI;
                var anguloFim = anguloAtual + deltaAngulo;

                var xs = cx + r * Math.Cos(anguloAtual);
                var ys = cy + r * Math.Sin(anguloAtual);
                var xe = cx + r * Math.Cos(anguloFim);
                var ye = cy + r * Math.Sin(anguloFim);

                var isLarge = deltaAngulo > Math.PI ? 1 : 0;
                var path = $"M {cx.ToString("F1", Inv)},{cy.ToString("F1", Inv)} L {xs.ToString("F1", Inv)},{ys.ToString("F1", Inv)} A {r.ToString("F1", Inv)},{r.ToString("F1", Inv)} 0 {isLarge},1 {xe.ToString("F1", Inv)},{ye.ToString("F1", Inv)} Z";

                var anguloMeio = anguloAtual + (deltaAngulo / 2);
                var isRight = Math.Cos(anguloMeio) >= 0;

                _pieSlices.Add(new PieSliceVisual
                {
                    Titulo = item.Titulo,
                    Quantidade = item.Quantidade,
                    Porcentagem = item.Porcentagem,
                    Cor = cor,
                    PathData = path,
                    IsRightSide = isRight,
                    AngleMid = anguloMeio,
                    RotuloPorcentagem = item.Porcentagem.ToString("N1", PtBr) + "%"
                });

                anguloAtual = anguloFim;
            }

            // Distribuição harmoniosa de altura das hastes para evitar colisão
            DistribuirHastesLado(_pieSlices.Where(s => s.IsRightSide).OrderBy(s => s.AngleMid).ToList(), isRight: true, cx, cy, r);
            DistribuirHastesLado(_pieSlices.Where(s => !s.IsRightSide).OrderBy(s => -s.AngleMid).ToList(), isRight: false, cx, cy, r);
        }

        private static void DistribuirHastesLado(List<PieSliceVisual> slices, bool isRight, double cx, double cy, double r)
        {
            var count = slices.Count;
            if (count == 0) return;

            const double yMin = 48.0;
            const double yMax = 222.0;

            for (int i = 0; i < count; i++)
            {
                var slice = slices[i];
                var targetY = count == 1
                    ? cy
                    : yMin + (i * ((yMax - yMin) / (count - 1)));

                // Ponto de início na borda do círculo
                slice.LineX1 = cx + r * Math.Cos(slice.AngleMid);
                slice.LineY1 = cy + r * Math.Sin(slice.AngleMid);

                // Joelho da haste saindo do círculo
                var kneeX = isRight ? cx + r + 16.0 : cx - r - 16.0;
                slice.LineX2 = kneeX;
                slice.LineY2 = targetY;

                // Extensão horizontal da haste
                var endX = isRight ? kneeX + 24.0 : kneeX - 24.0;
                slice.LineX3 = endX;
                slice.LineY3 = targetY;

                // Texto com porcentagem
                slice.TextX = isRight ? endX + 4.0 : endX - 4.0;
                slice.TextY = targetY + 4.0;
                slice.TextAnchor = isRight ? "start" : "end";
            }
        }

        #endregion
    }
}
