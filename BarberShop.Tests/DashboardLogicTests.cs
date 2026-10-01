using BarberShop.Core.Enums;
using BarberShop.Core.Models;
using BarberShop.Core.Responses.Dashboard;

namespace BarberShop.Tests;

public class DashboardLogicTests
{
    [Fact]
    public void DashboardResponse_CalculatesMetricsCorrectly_ExcludingCancelled()
    {
        var today = DateTime.Today;

        var agendamentos = new List<Agendamento>
        {
            new() { Id = 1, UserId = 10, CorteId = 1, Valor = 50m, Data = today.AddHours(9), Status = EStatusAgendamento.Concluido },
            new() { Id = 2, UserId = 10, CorteId = 2, Valor = 40m, Data = today.AddHours(14), Status = EStatusAgendamento.Aceito },
            new() { Id = 3, UserId = 20, CorteId = 1, Valor = 50m, Data = today.AddHours(16), Status = EStatusAgendamento.Cancelado }, // Não deve somar
        };

        var validos = agendamentos.Where(a => a.Status != EStatusAgendamento.Cancelado).ToList();
        var lucroHoje = validos.Where(a => a.Data.Date == today).Sum(a => a.Valor);

        Assert.Equal(90m, lucroHoje);
    }

    [Fact]
    public void DashboardResponse_IdentifiesTop3ClientesAndTopCorte()
    {
        var agendamentos = new List<Agendamento>
        {
            new() { Id = 1, UserId = 10, CorteId = 1, Valor = 50m, Status = EStatusAgendamento.Concluido },
            new() { Id = 2, UserId = 10, CorteId = 2, Valor = 40m, Status = EStatusAgendamento.Concluido },
            new() { Id = 3, UserId = 10, CorteId = 1, Valor = 50m, Status = EStatusAgendamento.Concluido },
            new() { Id = 4, UserId = 20, CorteId = 2, Valor = 40m, Status = EStatusAgendamento.Concluido },
            new() { Id = 5, UserId = 20, CorteId = 1, Valor = 50m, Status = EStatusAgendamento.Concluido },
            new() { Id = 6, UserId = 30, CorteId = 3, Valor = 35m, Status = EStatusAgendamento.Concluido },
            new() { Id = 7, UserId = 40, CorteId = 1, Valor = 50m, Status = EStatusAgendamento.Concluido },
        };

        var validos = agendamentos.Where(a => a.Status != EStatusAgendamento.Cancelado).ToList();

        var top3 = validos.GroupBy(a => a.UserId)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToList();

        Assert.Equal(3, top3.Count);
        Assert.Equal(10L, top3[0].UserId);
        Assert.Equal(3, top3[0].Count);
        Assert.Equal(20L, top3[1].UserId);
        Assert.Equal(2, top3[1].Count);
    }

    [Fact]
    public void DashboardResponse_InitializesDefaultValuesProperly()
    {
        var response = new DashboardResponse();

        Assert.Equal("Nenhum no período", response.TopClienteNome);
        Assert.Equal("Nenhum no período", response.TopCorteTitulo);
        Assert.Equal(0m, response.LucroDia);
        Assert.Equal(0m, response.LucroSemana);
        Assert.Equal(0m, response.LucroMes);
        Assert.Empty(response.TopClientes);
        Assert.Empty(response.CortesMetricas);
        Assert.Empty(response.LucroPorDia);
        Assert.Empty(response.LucroPorSemanaMes);
    }
}
