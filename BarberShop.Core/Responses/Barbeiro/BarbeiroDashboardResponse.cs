using BarberShop.Core.Responses.Agendamento;
using BarberShop.Core.Responses.Avaliacao;
using BarberShop.Core.Responses.Dashboard;

namespace BarberShop.Core.Responses.Barbeiro
{
    public class BarbeiroDashboardResponse
    {
        public long BarbeiroId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string FotoUrl { get; set; } = string.Empty;
        public string FilialNome { get; set; } = string.Empty;

        // Contadores de Agendamentos
        public int TotalAgendamentosHoje { get; set; }
        public int TotalAgendamentosSemana { get; set; }
        public int TotalAgendamentosMes { get; set; }

        // Lucros
        public decimal LucroHoje { get; set; }
        public decimal LucroSemana { get; set; }
        public decimal LucroMes { get; set; }

        // Serviço mais trabalhado
        public string ServicoMaisTrabalhadoTitulo { get; set; } = "Nenhum no período";
        public int ServicoMaisTrabalhadoQuantidade { get; set; }

        // Gráficos de lucro por período
        public List<PeriodoLucroItemResponse> LucroPorTurnoHoje { get; set; } = [];
        public List<PeriodoLucroItemResponse> LucroPorDia { get; set; } = [];
        public List<PeriodoLucroItemResponse> LucroPorSemanaMes { get; set; } = [];

        // Serviços atendidos (distribuição)
        public List<CorteMetricaResponse> ServicosMaisTrabalhados { get; set; } = [];
        public List<CorteMetricaResponse> ServicosHoje { get; set; } = [];

        // Agendamentos em modo leitura (Hoje, Semana, Mês e Histórico Completo)
        public List<AgendamentoResponse> AgendamentosHoje { get; set; } = [];
        public List<AgendamentoResponse> AgendamentosSemana { get; set; } = [];
        public List<AgendamentoResponse> AgendamentosMes { get; set; } = [];
        public List<AgendamentoResponse> TodosAgendamentos { get; set; } = [];

        // Avaliações
        public double MediaAvaliacoes { get; set; }
        public int TotalAvaliacoes { get; set; }
        public List<AvaliacaoResponse> MinhasAvaliacoes { get; set; } = [];
    }
}
