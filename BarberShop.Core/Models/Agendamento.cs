using BarberShop.Core.Enums;

namespace BarberShop.Core.Models
{
    public class Agendamento
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long CorteId { get; set; }
        public DateTime Data { get; set; }
        public TimeSpan Tempo { get; set; }
        public decimal Valor { get; set; }
        public EStatusAgendamento Status { get; set; } = EStatusAgendamento.Pendente;
        
        public virtual Corte Corte { get; set; } = null!;
    }
}
