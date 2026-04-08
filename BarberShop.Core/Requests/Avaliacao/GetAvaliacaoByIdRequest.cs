namespace BarberShop.Core.Requests.Avaliacao
{
    public class GetAvaliacaoByIdRequest : Request
    {
        public long UserId { get; set; }
        public long Id { get; set; }
    }
}
