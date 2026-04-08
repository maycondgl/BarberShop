namespace BarberShop.Core.Requests.Avaliacao
{
    public class GetAllAvaliacaoRequest : PagedRequest
    {
        public long UserId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
