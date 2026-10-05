namespace BarberShop.Core.Responses.Dashboard;

public class DashboardResponse
{
    public string TopClienteNome { get; set; } = "Nenhum no período";
    public int TopClienteAgendamentos { get; set; }
    public List<ClienteMetricaResponse> TopClientes { get; set; } = [];

    public string TopCorteTitulo { get; set; } = "Nenhum no período";
    public int TopCorteAgendamentos { get; set; }

    public decimal LucroDia { get; set; }
    public decimal LucroSemana { get; set; }
    public decimal LucroMes { get; set; }

    public int TotalAgendamentosHoje { get; set; }
    public int TotalAgendamentosSemana { get; set; }
    public int TotalAgendamentosMes { get; set; }

    public List<PeriodoLucroItemResponse> LucroPorTurnoHoje { get; set; } = [];
    public List<PeriodoLucroItemResponse> LucroPorDia { get; set; } = [];
    public List<PeriodoLucroItemResponse> LucroPorSemanaMes { get; set; } = [];

    public List<CorteMetricaResponse> CortesMetricas { get; set; } = [];
    public List<BarbeiroMetricaResponse> BarbeirosMetricas { get; set; } = [];
}

public class ClienteMetricaResponse
{
    public long UserId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int TotalAgendamentos { get; set; }
    public decimal TotalGasto { get; set; }
}

public class PeriodoLucroItemResponse
{
    public string Rotulo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int Quantidade { get; set; }
}

public class CorteMetricaResponse
{
    public long CorteId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal TotalReceita { get; set; }
    public double Porcentagem { get; set; }
}

public class BarbeiroMetricaResponse
{
    public long BarbeiroId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string FotoUrl { get; set; } = string.Empty;
    public int TotalAgendamentos { get; set; }
    public decimal TotalLucro { get; set; }
    public double Porcentagem { get; set; }
}
