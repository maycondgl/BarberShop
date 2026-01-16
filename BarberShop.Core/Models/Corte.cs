namespace BarberShop.Core.Models
{
    public class Corte
    {
        public long Id { get; set; }
        public string? Titulo { get; set; }
        public decimal Preco { get; set; }
        public int DuracaoMinutos { get; set; }
        public string? UserId { get; set; }
    }
}
