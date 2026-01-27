namespace BarberShop.Core.Models
{
    public class Corte
    {
        public long Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int DuracaoMinutos { get; set; }
    }
}
