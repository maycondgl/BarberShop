namespace BarberShop.Core.Requests.Avaliacao
{
    public class GetAvaliacaoByIdRequest : Request
    {
        public new long UserId { get; set; }
        public long Id { get; set; }
    }
}
