using System;

namespace BarberShop.Core.Models
{
    public class HorarioFuncionamento
    {
        public long Id { get; set; }
        public long FilialId { get; set; }
        public virtual Filial? Filial { get; set; }
        public DayOfWeek DiaSemana { get; set; }
        public bool Aberto { get; set; } = true;
        public TimeSpan HorarioAbertura { get; set; } = new(8, 0, 0);
        public TimeSpan HorarioFechamento { get; set; } = new(19, 0, 0);
        public bool TemAlmoco { get; set; } = true;
        public TimeSpan? AlmocoInicio { get; set; } = new(12, 0, 0);
        public TimeSpan? AlmocoFim { get; set; } = new(13, 0, 0);
    }
}
