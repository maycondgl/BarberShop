namespace BarberShop.Core.Requests.Agendamentos
{
    public class GetAllAgendamentoRequest : PagedRequest
    {
        public new long UserId { get; set; }
        public new int PageNumber { get; set; }
        public new int PageSize { get; set; }
    }
}

