namespace BarberShop.Core.Models.Account
{
    public class MeResponse
    {
        public string? Name { get; set; }
        public bool IsAuthenticated { get; set; }
        public List<MeClaim> Claims { get; set; } = new();
    }

    public class MeClaim
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}