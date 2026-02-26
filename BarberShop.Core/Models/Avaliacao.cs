namespace BarberShop.Core.Models
{
    public class Avaliacao
    {
        public long Id { get; set; }
        public long ClienteId { get; set; }
        public int Estrelas { get; set; }
        public string? Comentario { get; set; } = string.Empty;
        public DateTime Data { get; set; } = DateTime.Now;
        public string UserId { get; set; } = string.Empty;

        public virtual Cliente Cliente { get; set; } = null!;
    }
}
