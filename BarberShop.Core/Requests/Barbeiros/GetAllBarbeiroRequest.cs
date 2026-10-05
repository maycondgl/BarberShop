namespace BarberShop.Core.Requests.Barbeiros
{
    public class GetAllBarbeiroRequest : PagedRequest
    {
        public long? FilialId { get; set; }
        public bool? ApenasAtivos { get; set; }
    }
}
