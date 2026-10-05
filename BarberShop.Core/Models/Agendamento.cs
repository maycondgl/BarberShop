using BarberShop.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

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

        public long? FilialId { get; set; }
        public virtual Filial? Filial { get; set; }

        public long? BarbeiroId { get; set; }
        public virtual Barbeiro? Barbeiro { get; set; }

        public string? DescricaoServicos { get; set; }
        
        [NotMapped]
        public string NomeCliente { get; set; } = string.Empty;
    }
}
