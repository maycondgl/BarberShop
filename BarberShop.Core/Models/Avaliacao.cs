namespace BarberShop.Core.Models
{
    public class Avaliacao
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long AgendamentoId { get; set; }
        public int Estrelas { get; set; }
        public string? Comentario { get; set; } = string.Empty;
        public DateTime Data { get; set; } = DateTime.Now;

    }
}
