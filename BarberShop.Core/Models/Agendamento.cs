namespace BarberShop.Core.Models
{
    public class Agendamento
    {
        public long Id { get; set; }
        public long ClienteId { get; set; }
        public long CorteId { get; set; }
        public DateTime Data { get; set; }
        public string Tempo { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public string UserId { get; set; } = string.Empty;
    }
}
