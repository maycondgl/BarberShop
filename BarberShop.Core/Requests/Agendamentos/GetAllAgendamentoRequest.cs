namespace BarberShop.Core.Requests.Agendamentos
{
    public class GetAllAgendamentoRequest : PagedRequest
    {
        public new long UserId { get; set; }
    }
}

