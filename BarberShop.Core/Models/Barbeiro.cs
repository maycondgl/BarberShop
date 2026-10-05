namespace BarberShop.Core.Models
{
    public class Barbeiro
    {
        public long Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string FotoUrl { get; set; } = string.Empty;
        public long? FilialId { get; set; }
        public virtual Filial? Filial { get; set; }
        public bool Ativo { get; set; } = true;
    }
}
