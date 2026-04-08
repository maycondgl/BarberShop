namespace BarberShop.Core.Requests.Agendamentos
{
    public class GetAllAgendamentoRequest : PagedRequest
    {
        public long UserId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}

