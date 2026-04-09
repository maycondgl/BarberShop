namespace BarberShop.Core.Requests.Avaliacao
{
    public class GetAllAvaliacaoRequest : PagedRequest
    {
        public new long UserId { get; set; }
        public new int PageNumber { get; set; }
        public new int PageSize { get; set; }
    }
}
