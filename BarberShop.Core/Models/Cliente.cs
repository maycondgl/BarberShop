namespace BarberShop.Core.Models
{
    public class Cliente
    {
        public long Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty; // Chave para identificar o cliente
        public string UserId { get; set; } = string.Empty;
    }
}

