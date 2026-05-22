namespace BarberShop.Core.Models
{
    public class Corte
    {
        public long Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int DuracaoMinutos { get; set; }
        public string Role { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;
        public string ImagemUrl { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;

    }
}
