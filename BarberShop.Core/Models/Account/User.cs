namespace BarberShop.Core.Models.Account
{
    public class User
    {
        public string Nome { get; set; } = string.Empty;
        public int Telefone { get; set; }
        public string Email { get; set; } = string.Empty;
        public Dictionary<string, string> Claims { get; set; } = [];
    }
}
