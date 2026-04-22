namespace BarberShop.Core.Requests.Agendamentos
{
    public class GetAgendamentoByPeriodRequest : PagedRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
