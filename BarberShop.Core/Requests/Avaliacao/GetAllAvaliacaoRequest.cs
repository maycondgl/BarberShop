namespace BarberShop.Core.Requests.Avaliacao
{
    public class GetAllAvaliacaoRequest : PagedRequest
    {
        public new long UserId { get; set; }
    }
}
