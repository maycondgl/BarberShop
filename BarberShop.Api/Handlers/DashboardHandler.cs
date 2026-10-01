using BarberShop.Api.Data;
using BarberShop.Api.Models;
using BarberShop.Core.Enums;
using BarberShop.Core.Extensions;
using BarberShop.Core.Handlers;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BarberShop.Api.Handlers;

public class DashboardHandler : IDashboardHandler
{
    private static readonly CultureInfo PtBr = new("pt-BR");
    private readonly BarberShopContext _context;
    private readonly UserManager<User> _userManager;

    public DashboardHandler(BarberShopContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Response<DashboardResponse?>> GetDashboardAsync()
    {
        try
        {
            var today = DateTime.Today;
            var todayEnd = today.AddDays(1).AddTicks(-1);

            var diffToMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var weekStart = today.AddDays(-diffToMonday);
            var weekEnd = weekStart.AddDays(7).AddTicks(-1);

            var monthStart = today.GetFirstDayOfMonth();
            var monthEnd = today.GetLastDayOfMonth().AddDays(1).AddTicks(-1);

            // Consultar agendamentos válidos (não cancelados)
            var agendamentosValidos = await _context.Agendamentos
                .AsNoTracking()
                .Where(a => a.Status != EStatusAgendamento.Cancelado)
                .Select(a => new
                {
                    a.Id,
                    a.UserId,
                    a.CorteId,
                    a.Data,
                    a.Valor
                })
                .ToListAsync();

            // Cálculos diários, semanais e mensais
            var hojeAgendamentos = agendamentosValidos
                .Where(a => a.Data >= today && a.Data <= todayEnd)
                .ToList();

            var semanaAgendamentos = agendamentosValidos
                .Where(a => a.Data >= weekStart && a.Data <= weekEnd)
                .ToList();

            var mesAgendamentos = agendamentosValidos
                .Where(a => a.Data >= monthStart && a.Data <= monthEnd)
                .ToList();

            var lucroDia = hojeAgendamentos.Sum(a => a.Valor);
            var lucroSemana = semanaAgendamentos.Sum(a => a.Valor);
            var lucroMes = mesAgendamentos.Sum(a => a.Valor);

            // 1. Top 3 Clientes que mais agendaram
            var topClientesGroups = agendamentosValidos
                .GroupBy(a => a.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count(), TotalGasto = g.Sum(x => x.Valor) })
                .OrderByDescending(x => x.Count)
                .ThenByDescending(x => x.TotalGasto)
                .Take(3)
                .ToList();

            var topClientes = new List<ClienteMetricaResponse>();
            foreach (var group in topClientesGroups)
            {
                var user = await _userManager.FindByIdAsync(group.UserId.ToString());
                var nome = !string.IsNullOrWhiteSpace(user?.NomeCompleto)
                    ? user.NomeCompleto
                    : (user?.UserName ?? $"Cliente #{group.UserId}");

                topClientes.Add(new ClienteMetricaResponse
                {
                    UserId = group.UserId,
                    Nome = nome,
                    TotalAgendamentos = group.Count,
                    TotalGasto = group.TotalGasto
                });
            }

            string topClienteNome = topClientes.FirstOrDefault()?.Nome ?? "Nenhum no período";
            int topClienteAgendamentos = topClientes.FirstOrDefault()?.TotalAgendamentos ?? 0;

            // 2. Corte mais escolhido
            var topCorteGroup = agendamentosValidos
                .GroupBy(a => a.CorteId)
                .Select(g => new { CorteId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            string topCorteTitulo = "Nenhum no período";
            int topCorteAgendamentos = 0;

            if (topCorteGroup != null)
            {
                topCorteAgendamentos = topCorteGroup.Count;
                var corte = await _context.Cortes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == topCorteGroup.CorteId);
                topCorteTitulo = corte?.Titulo ?? $"Corte #{topCorteGroup.CorteId}";
            }

            // 3. Métricas por corte do catálogo (com porcentagem)
            var allCortes = await _context.Cortes
                .AsNoTracking()
                .OrderBy(c => c.Titulo)
                .ToListAsync();

            var corteCounts = agendamentosValidos
                .GroupBy(a => a.CorteId)
                .ToDictionary(g => g.Key, g => new { Count = g.Count(), TotalReceita = g.Sum(x => x.Valor) });

            var totalAgendamentosCortes = agendamentosValidos.Count;

            var cortesMetricas = allCortes.Select(c =>
            {
                corteCounts.TryGetValue(c.Id, out var stat);
                var qtd = stat?.Count ?? 0;
                var pct = totalAgendamentosCortes > 0
                    ? Math.Round(((double)qtd / totalAgendamentosCortes) * 100, 1)
                    : 0.0;

                return new CorteMetricaResponse
                {
                    CorteId = c.Id,
                    Titulo = c.Titulo,
                    Quantidade = qtd,
                    TotalReceita = stat?.TotalReceita ?? 0,
                    Porcentagem = pct
                };
            }).OrderByDescending(x => x.Quantidade).ToList();

            // 4. Detalhamento de Lucro: Por Turno Hoje
            var lucroPorTurnoHoje = new List<PeriodoLucroItemResponse>
            {
                new()
                {
                    Rotulo = "Manhã (08h-12h)",
                    Valor = hojeAgendamentos.Where(a => a.Data.Hour < 12).Sum(a => a.Valor),
                    Quantidade = hojeAgendamentos.Count(a => a.Data.Hour < 12)
                },
                new()
                {
                    Rotulo = "Tarde (12h-17h)",
                    Valor = hojeAgendamentos.Where(a => a.Data.Hour >= 12 && a.Data.Hour < 17).Sum(a => a.Valor),
                    Quantidade = hojeAgendamentos.Count(a => a.Data.Hour >= 12 && a.Data.Hour < 17)
                },
                new()
                {
                    Rotulo = "Noite (17h-19h)",
                    Valor = hojeAgendamentos.Where(a => a.Data.Hour >= 17).Sum(a => a.Valor),
                    Quantidade = hojeAgendamentos.Count(a => a.Data.Hour >= 17)
                }
            };

            // 5. Detalhamento de Lucro: Últimos 7 dias
            var lucroPorDia = new List<PeriodoLucroItemResponse>();
            for (int i = 6; i >= 0; i--)
            {
                var dia = today.AddDays(-i);
                var diaFim = dia.AddDays(1).AddTicks(-1);
                var diaAgendamentos = agendamentosValidos.Where(a => a.Data >= dia && a.Data <= diaFim).ToList();
                var diaNomeRaw = dia.ToString("ddd", PtBr);
                var diaNome = diaNomeRaw.Length > 0
                    ? char.ToUpper(diaNomeRaw[0]) + diaNomeRaw[1..]
                    : diaNomeRaw;

                lucroPorDia.Add(new PeriodoLucroItemResponse
                {
                    Rotulo = $"{diaNome} ({dia:dd/MM})",
                    Valor = diaAgendamentos.Sum(a => a.Valor),
                    Quantidade = diaAgendamentos.Count
                });
            }

            // 6. Detalhamento de Lucro: Semanas do Mês Atual
            var lucroPorSemanaMes = new List<PeriodoLucroItemResponse>();
            var totalDiasMes = DateTime.DaysInMonth(today.Year, today.Month);
            var semanasDef = new (int inicio, int fim, string rotulo)[]
            {
                (1, 7, "Sem 1 (01-07)"),
                (8, 14, "Sem 2 (08-14)"),
                (15, 21, "Sem 3 (15-21)"),
                (22, totalDiasMes, $"Sem 4 (22-{totalDiasMes:D2})")
            };

            foreach (var (inicio, fim, rotulo) in semanasDef)
            {
                var inicioData = new DateTime(today.Year, today.Month, inicio);
                var fimData = new DateTime(today.Year, today.Month, fim, 23, 59, 59);
                var agendamentosSemana = mesAgendamentos.Where(a => a.Data >= inicioData && a.Data <= fimData).ToList();
                lucroPorSemanaMes.Add(new PeriodoLucroItemResponse
                {
                    Rotulo = rotulo,
                    Valor = agendamentosSemana.Sum(a => a.Valor),
                    Quantidade = agendamentosSemana.Count
                });
            }

            var response = new DashboardResponse
            {
                TopClienteNome = topClienteNome,
                TopClienteAgendamentos = topClienteAgendamentos,
                TopClientes = topClientes,
                TopCorteTitulo = topCorteTitulo,
                TopCorteAgendamentos = topCorteAgendamentos,
                LucroDia = lucroDia,
                LucroSemana = lucroSemana,
                LucroMes = lucroMes,
                TotalAgendamentosHoje = hojeAgendamentos.Count,
                TotalAgendamentosSemana = semanaAgendamentos.Count,
                TotalAgendamentosMes = mesAgendamentos.Count,
                LucroPorTurnoHoje = lucroPorTurnoHoje,
                LucroPorDia = lucroPorDia,
                LucroPorSemanaMes = lucroPorSemanaMes,
                CortesMetricas = cortesMetricas
            };

            return new Response<DashboardResponse?>(response, 200, "Métricas carregadas com sucesso");
        }
        catch (Exception ex)
        {
            return new Response<DashboardResponse?>(null, 500, $"Erro ao consolidar dados do dashboard: {ex.Message}");
        }
    }
}
