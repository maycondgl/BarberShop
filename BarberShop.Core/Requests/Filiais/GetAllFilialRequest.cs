namespace BarberShop.Core.Requests.Filiais
{
    public class GetAllFilialRequest : PagedRequest
    {
        public bool? ApenasAtivos { get; set; }
    }
}
