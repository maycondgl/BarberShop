using System.ComponentModel.DataAnnotations;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Requests.DiasFechados;

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
                Data = DateTime.Today.AddDays(1)
            };

            var errors = Validate(request);

            Assert.Equal(2, errors.Count);
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
            Validator.TryValidateObject(model, new ValidationContext(model), results, true);
            return results;
        }
    }
}
