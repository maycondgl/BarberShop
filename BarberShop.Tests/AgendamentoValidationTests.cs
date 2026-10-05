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
                Localizacao = ""
            };

            var errors = Validate(request);

            Assert.Equal(2, errors.Count);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateFilialRequest.Nome)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateFilialRequest.Localizacao)));
        }

        [Fact]
        public void CreateBarbeiroRequest_ValidData_PassesValidation()
        {
            var request = new CreateBarbeiroRequest
            {
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
        public void CreateDiaFechadoRequest_ExceedingMotivoLength_FailsValidation()
        {
            var request = new CreateDiaFechadoRequest
            {
                Data = DateTime.Today.AddDays(5),
                Motivo = new string('A', 151)
            };

            var errors = Validate(request);

            Assert.Single(errors);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateDiaFechadoRequest.Motivo)));
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
