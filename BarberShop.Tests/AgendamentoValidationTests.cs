using System.ComponentModel.DataAnnotations;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Requests.Barbeiros;
using BarberShop.Core.Requests.DiasFechados;
using BarberShop.Core.Requests.Filiais;

namespace BarberShop.Tests
{
    public class AgendamentoValidationTests
    {
        [Fact]
        public void CreateAgendamentoRequest_ValidData_PassesValidation()
        {
            var request = new CreateAgendamentoRequest
            {
                UserId = 1,
                CorteId = 2,
                FilialId = 1,
                BarbeiroId = 1,
                Data = DateTime.Today.AddDays(2).AddHours(10)
            };

            var errors = Validate(request);

            Assert.Empty(errors);
        }

        [Fact]
        public void CreateAgendamentoRequest_InvalidUserOrCorte_FailsValidation()
        {
            var request = new CreateAgendamentoRequest
            {
                UserId = 0,
                CorteId = -1,
                FilialId = 1,
                BarbeiroId = 1,
                Data = DateTime.Today.AddDays(1)
            };

            var errors = Validate(request);

            Assert.Equal(2, errors.Count);
        }

        [Fact]
        public void CreateAgendamentoRequest_MissingFilialOrBarbeiro_FailsValidation()
        {
            var request = new CreateAgendamentoRequest
            {
                UserId = 1,
                CorteId = 1,
                FilialId = null,
                BarbeiroId = null,
                Data = DateTime.Today.AddDays(2)
            };

            var errors = Validate(request);

            Assert.Equal(2, errors.Count);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateAgendamentoRequest.FilialId)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateAgendamentoRequest.BarbeiroId)));
        }

        [Fact]
        public void CreateFilialRequest_ValidData_PassesValidation()
        {
            var request = new CreateFilialRequest
            {
                Nome = "BarberShop - Centro",
                Localizacao = "Rua Central, 100",
                Telefone = "(11) 99999-9999"
            };

            var errors = Validate(request);

            Assert.Empty(errors);
        }

        [Fact]
        public void CreateFilialRequest_MissingNomeOrLocalizacao_FailsValidation()
        {
            var request = new CreateFilialRequest
            {
                Nome = "",
                Localizacao = "",
                Telefone = "(11) 99999-9999"
            };

            var errors = Validate(request);

            Assert.Equal(2, errors.Count);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateFilialRequest.Nome)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateFilialRequest.Localizacao)));
        }

        [Fact]
        public void CreateFilialRequest_InvalidTelefone_FailsValidation()
        {
            var request = new CreateFilialRequest
            {
                Nome = "BarberShop - Centro",
                Localizacao = "Rua Central, 100",
                Telefone = "12345" // Telefone curto/inválido
            };

            var errors = Validate(request);

            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateFilialRequest.Telefone)));
        }

        [Fact]
        public void CreateBarbeiroRequest_InvalidEmailProvider_FailsValidation()
        {
            var request = new CreateBarbeiroRequest
            {
                Email = "barbeiro@provedordesconhecido.xyz",
                Nome = "Lucas Silva",
                FilialId = 1
            };

            var errors = Validate(request);

            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateBarbeiroRequest.Email)));
        }

        [Fact]
        public void CreateBarbeiroRequest_ValidData_PassesValidation()
        {
            var request = new CreateBarbeiroRequest
            {
                Email = "lucas@barbershop.com",
                Nome = "Lucas Silva",
                FilialId = 1
            };

            var errors = Validate(request);

            Assert.Empty(errors);
        }

        [Fact]
        public void CreateBarbeiroRequest_MissingNome_FailsValidation()
        {
            var request = new CreateBarbeiroRequest
            {
                Email = "lucas@barbershop.com",
                Nome = ""
            };

            var errors = Validate(request);

            Assert.Single(errors);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateBarbeiroRequest.Nome)));
        }

        [Fact]
        public void CreateDiaFechadoRequest_ValidData_PassesValidation()
        {
            var request = new CreateDiaFechadoRequest
            {
                Data = DateTime.Today.AddDays(5),
                Motivo = "Feriado Municipal"
            };

            var errors = Validate(request);

            Assert.Empty(errors);
        }

        [Fact]
        public void UpdateFilialRequest_ValidData_PassesValidation()
        {
            var request = new UpdateFilialRequest
            {
                Id = 1,
                Nome = "BarberShop - Shopping",
                Localizacao = "Av. Principal, 500",
                Telefone = "(11) 98888-7777"
            };

            var errors = Validate(request);

            Assert.Empty(errors);
        }

        [Fact]
        public void UpdateFilialRequest_InvalidTelefone_FailsValidation()
        {
            var request = new UpdateFilialRequest
            {
                Id = 1,
                Nome = "BarberShop - Shopping",
                Localizacao = "Av. Principal, 500",
                Telefone = "00999999999" // DDD 00 inválido
            };

            var errors = Validate(request);

            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(UpdateFilialRequest.Telefone)));
        }

        [Theory]
        [InlineData("11:00", "11:40", false)] // Antes do almoço -> Sem conflito
        [InlineData("11:30", "12:10", true)]  // Termina dentro do almoço -> Conflito
        [InlineData("12:00", "12:40", true)]  // Começa no início do almoço -> Conflito
        [InlineData("12:30", "13:10", true)]  // Começa no almoço e termina depois -> Conflito
        [InlineData("13:00", "13:40", false)] // Inicia exatamente no fim do almoço -> Sem conflito
        public void HorarioFuncionamento_LunchConflictLogic_CorrectlyIdentifiesOverlaps(
            string slotInicioStr, string slotFimStr, bool conflitoEsperado)
        {
            var almocoInicio = TimeSpan.Parse("12:00");
            var almocoFim = TimeSpan.Parse("13:00");

            var slotInicio = TimeSpan.Parse(slotInicioStr);
            var slotFim = TimeSpan.Parse(slotFimStr);

            bool temConflito = slotInicio < almocoFim && slotFim > almocoInicio;

            Assert.Equal(conflitoEsperado, temConflito);
        }

        [Fact]
        public void HorarioFuncionamentoDto_ObterNomeDia_ReturnsCorrectPortugueseNames()
        {
            Assert.Equal("Domingo", BarberShop.Core.Requests.HorariosFuncionamento.HorarioFuncionamentoDto.ObterNomeDia(DayOfWeek.Sunday));
            Assert.Equal("Segunda-feira", BarberShop.Core.Requests.HorariosFuncionamento.HorarioFuncionamentoDto.ObterNomeDia(DayOfWeek.Monday));
            Assert.Equal("Sábado", BarberShop.Core.Requests.HorariosFuncionamento.HorarioFuncionamentoDto.ObterNomeDia(DayOfWeek.Saturday));
        }

        private static List<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }
    }
}
