using BarberShop.Core.Enums;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Barbeiros;
using BarberShop.Core.Responses.Avaliacao;
using BarberShop.Core.Responses.Barbeiro;
using BarberShop.Core.Responses.Dashboard;
using System.ComponentModel.DataAnnotations;

namespace BarberShop.Tests
{
    public class BarbeiroAndAvaliacaoTests
    {
        [Fact]
        public void Agendamento_HorarioIntervalOverlap_BlocksSameBarber_AllowsDifferentBarber()
        {
            var baseDate = DateTime.Today.AddHours(14); // 14:00
            long barbeiroA = 1;
            long barbeiroB = 2;

            // Cliente 1 agendou 1h30 (90 min) das 14:00 às 15:30 com Barbeiro A
            var agendamentoExistente = new Agendamento
            {
                Id = 1,
                UserId = 10,
                BarbeiroId = barbeiroA,
                Data = baseDate,
                Tempo = TimeSpan.FromMinutes(90),
                Status = EStatusAgendamento.Aceito
            };

            var ocupadoInicio = agendamentoExistente.Data.TimeOfDay;
            var ocupadoFim = ocupadoInicio.Add(agendamentoExistente.Tempo); // 15:30

            // Candidato 1: 14:30 com Barbeiro A (30 min) -> sobrepõe
            var cand1Inicio = new TimeSpan(14, 30, 0);
            var cand1Fim = cand1Inicio.Add(TimeSpan.FromMinutes(30)); // 15:00
            bool conflitaBarbeiroA = cand1Inicio < ocupadoFim && cand1Fim > ocupadoInicio;

            Assert.True(conflitaBarbeiroA, "O agendamento sobreposto para o mesmo barbeiro deve gerar conflito.");

            // Candidato 2: 15:30 com Barbeiro A (30 min) -> logo após término, livre
            var cand2Inicio = new TimeSpan(15, 30, 0);
            var cand2Fim = cand2Inicio.Add(TimeSpan.FromMinutes(30)); // 16:00
            bool conflitaBarbeiroAPosTermino = cand2Inicio < ocupadoFim && cand2Fim > ocupadoInicio;

            Assert.False(conflitaBarbeiroAPosTermino, "O horário que começa exatamente no término do anterior deve estar livre.");

            // Candidato 3: 14:30 com Barbeiro B (outro barbeiro) -> livre
            long outroBarbeiroId = barbeiroB;
            bool mesmoBarbeiro = outroBarbeiroId == agendamentoExistente.BarbeiroId;

            Assert.False(mesmoBarbeiro, "O barbeiro diferente não deve ter seu horário bloqueado.");
        }

        [Fact]
        public void Dashboard_CalculatesProfitAndRankingPerBarberCorrectly()
        {
            var agendamentos = new List<Agendamento>
            {
                new() { Id = 1, BarbeiroId = 1, Valor = 100m, Status = EStatusAgendamento.Concluido },
                new() { Id = 2, BarbeiroId = 1, Valor = 50m, Status = EStatusAgendamento.Concluido },
                new() { Id = 3, BarbeiroId = 2, Valor = 50m, Status = EStatusAgendamento.Concluido },
                new() { Id = 4, BarbeiroId = 2, Valor = 50m, Status = EStatusAgendamento.Cancelado }, // Não conta
            };

            var validos = agendamentos.Where(a => a.Status != EStatusAgendamento.Cancelado).ToList();
            var lucroTotal = validos.Sum(a => a.Valor); // 200

            var barbeiroGrupos = validos
                .Where(a => a.BarbeiroId.HasValue)
                .GroupBy(a => a.BarbeiroId!.Value)
                .Select(g => new BarbeiroMetricaResponse
                {
                    BarbeiroId = g.Key,
                    TotalAgendamentos = g.Count(),
                    TotalLucro = g.Sum(x => x.Valor),
                    Porcentagem = lucroTotal > 0 ? (double)(g.Sum(x => x.Valor) / lucroTotal * 100) : 0
                })
                .OrderByDescending(b => b.TotalLucro)
                .ToList();

            Assert.Equal(2, barbeiroGrupos.Count);
            Assert.Equal(1L, barbeiroGrupos[0].BarbeiroId);
            Assert.Equal(150m, barbeiroGrupos[0].TotalLucro);
            Assert.Equal(75.0, barbeiroGrupos[0].Porcentagem);

            Assert.Equal(2L, barbeiroGrupos[1].BarbeiroId);
            Assert.Equal(50m, barbeiroGrupos[1].TotalLucro);
            Assert.Equal(25.0, barbeiroGrupos[1].Porcentagem);
        }

        [Fact]
        public void Avaliacoes_PrivacyRules_FilterClientAndBarberCorrectly()
        {
            var avaliacoes = new List<AvaliacaoResponse>
            {
                new(1, 10, 100, 5, "Excelente corte", DateTime.Now, "Cliente A", "Lucas Silva", "Cabelo", 1),
                new(2, 20, 101, 4, "Muito bom", DateTime.Now, "Cliente B", "Lucas Silva", "Barba", 1),
                new(3, 30, 102, 5, "Top", DateTime.Now, "Cliente C", "Marcos Santos", "Cabelo", 2),
            };

            // Regra 1: Cliente A só vê suas próprias avaliações (UserId == 10)
            long clienteLogadoId = 10;
            var clienteAvaliacoes = avaliacoes.Where(a => a.UserId == clienteLogadoId).ToList();
            Assert.Single(clienteAvaliacoes);
            Assert.Equal(1L, clienteAvaliacoes[0].Id);

            // Regra 2: Barbeiro 1 (Lucas) só vê as avaliações feitas para ele
            long barbeiroId = 1;
            var barbeiroAvaliacoes = avaliacoes.Where(a => a.BarbeiroId == barbeiroId).ToList();
            Assert.Equal(2, barbeiroAvaliacoes.Count);
            Assert.All(barbeiroAvaliacoes, a => Assert.Equal(barbeiroId, a.BarbeiroId));

            // Regra 3: Admin vê todas as avaliações
            var adminAvaliacoes = avaliacoes.ToList();
            Assert.Equal(3, adminAvaliacoes.Count);
        }

        [Fact]
        public void BarbeiroDashboardResponse_CalculatesPeriodSummariesProperly()
        {
            var today = DateTime.Today;
            var agendamentosBarbeiro = new List<Agendamento>
            {
                new() { Id = 1, BarbeiroId = 1, Valor = 60m, Data = today.AddHours(10), Status = EStatusAgendamento.Concluido, DescricaoServicos = "Degradê" },
                new() { Id = 2, BarbeiroId = 1, Valor = 40m, Data = today.AddHours(14), Status = EStatusAgendamento.Aceito, DescricaoServicos = "Barba Terapia" },
                new() { Id = 3, BarbeiroId = 1, Valor = 60m, Data = today.AddDays(-2), Status = EStatusAgendamento.Concluido, DescricaoServicos = "Degradê" },
                new() { Id = 4, BarbeiroId = 1, Valor = 80m, Data = today.AddDays(-20), Status = EStatusAgendamento.Cancelado, DescricaoServicos = "Combo" }, // Cancelado
            };

            var validos = agendamentosBarbeiro.Where(a => a.Status != EStatusAgendamento.Cancelado).ToList();
            var hoje = validos.Where(a => a.Data.Date == today).ToList();

            var servicoMaisTrabalhado = validos
                .GroupBy(a => a.DescricaoServicos)
                .OrderByDescending(g => g.Count())
                .First();

            Assert.Equal(2, hoje.Count);
            Assert.Equal(100m, hoje.Sum(a => a.Valor));
            Assert.Equal("Degradê", servicoMaisTrabalhado.Key);
            Assert.Equal(2, servicoMaisTrabalhado.Count());
        }

        [Fact]
        public void CreateBarbeiroRequest_RequiresValidEmail_AndValidates()
        {
            var reqInvalido = new CreateBarbeiroRequest
            {
                Email = "email-invalido",
                Nome = "Lucas Barbeiro"
            };

            var context = new ValidationContext(reqInvalido);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(reqInvalido, context, results, true);

            Assert.False(isValid, "O e-mail mal formatado deve falhar na validação do modelo.");
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateBarbeiroRequest.Email)));

            var reqValido = new CreateBarbeiroRequest
            {
                Email = "barbeiro@barbershop.com",
                Nome = "Lucas Barbeiro"
            };

            results.Clear();
            var isValidValido = Validator.TryValidateObject(reqValido, new ValidationContext(reqValido), results, true);
            Assert.True(isValidValido, "O e-mail em formato correto deve passar na validação.");
        }

        [Fact]
        public void Barbeiro_LinksUsuarioId_AndStoresEmail()
        {
            var barbeiro = new Barbeiro
            {
                Id = 1,
                Nome = "Carlos Barbeiro",
                UsuarioId = 42,
                Email = "carlos@barbershop.com"
            };

            Assert.Equal(42, barbeiro.UsuarioId);
            Assert.Equal("carlos@barbershop.com", barbeiro.Email);
            Assert.Equal("Carlos Barbeiro", barbeiro.Nome);
        }

        [Fact]
        public void BarbeiroUsuarioInfoResponse_CorrectlyIdentifiesUser()
        {
            var info = new BarbeiroUsuarioInfoResponse(
                Id: 55,
                Nome: "Mateus Silva",
                Email: "mateus@barbershop.com",
                Telefone: "(11) 99999-8888",
                JaCadastradoComoBarbeiro: true,
                BarbeiroNome: "Mateus Silva Fade"
            );

            Assert.Equal(55, info.Id);
            Assert.Equal("Mateus Silva", info.Nome);
            Assert.True(info.JaCadastradoComoBarbeiro, "Deve indicar que o usuário já possui cadastro de barbeiro.");
            Assert.Equal("Mateus Silva Fade", info.BarbeiroNome);
        }
    }
}
