namespace BarberShop.Core.Models
{
    public class Avaliacao
    {
        public long Id { get; set; }
        public int Estrelas { get; set; } // Nota de 1 a 5
        public string Comentario { get; set; } = string.Empty;
        public long ClienteId { get; set; } // Referência ao cliente que avaliou
        public DateTime Data { get; set; } = DateTime.Now;

        public virtual Cliente Cliente { get; set; } = null!;
    }
}
