using System;

namespace BarberShop.Core.Requests.HorariosFuncionamento
{
    public class HorarioFuncionamentoDto
    {
        public long Id { get; set; }
        public long FilialId { get; set; }
        public DayOfWeek DiaSemana { get; set; }
        public string NomeDiaSemana { get; set; } = string.Empty;
        public bool Aberto { get; set; } = true;
        public TimeSpan HorarioAbertura { get; set; } = new(8, 0, 0);
        public TimeSpan HorarioFechamento { get; set; } = new(19, 0, 0);
        public bool TemAlmoco { get; set; } = true;
        public TimeSpan? AlmocoInicio { get; set; } = new(12, 0, 0);
        public TimeSpan? AlmocoFim { get; set; } = new(13, 0, 0);

        public static string ObterNomeDia(DayOfWeek dia) => dia switch
        {
            DayOfWeek.Sunday => "Domingo",
            DayOfWeek.Monday => "Segunda-feira",
            DayOfWeek.Tuesday => "Terça-feira",
            DayOfWeek.Wednesday => "Quarta-feira",
            DayOfWeek.Thursday => "Quinta-feira",
            DayOfWeek.Friday => "Sexta-feira",
            DayOfWeek.Saturday => "Sábado",
            _ => dia.ToString()
        };
    }
}
