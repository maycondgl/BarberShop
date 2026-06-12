namespace BarberShop.Api.Models
{
    public class Secrets
    {
        public string JwtTokenSecret { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;

    }
}
