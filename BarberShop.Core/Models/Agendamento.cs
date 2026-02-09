using BarberShop.Core.Enums;

namespace BarberShop.Core.Models
{
    public class Agendamento
    {
        public long Id { get; set; }
        public long ClienteId { get; set; }
        public long CorteId { get; set; }
        public DateTime Data { get; set; }
        public TimeSpan Tempo { get; set; }
        public decimal Valor { get; set; }
        public EStatusAgendamento Status { get; set; } = EStatusAgendamento.Pendente;
        public string UserId { get; set; } = string.Empty;

        public virtual Cliente Cliente { get; set; } = null!;
        public virtual Corte Corte { get; set; } = null!;
    }
}
