namespace BarberShop.Core.Responses.Account
{
    public class AdminUserResponse
    {
        public long Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public bool IsAdmin { get; set; }
    }
}
