using BarberShop.Core.Enums;

namespace BarberShop.Core.Requests.Agendamentos
{
    public class UpdateStatusAgendamentoRequest
    {
        public long Id { get; set; }
        public EStatusAgendamento Status { get; set; }
    }
}
