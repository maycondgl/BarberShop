using System.Collections.Generic;

namespace BarberShop.Core.Requests.HorariosFuncionamento
{
    public class SalvarHorariosFilialRequest : Request
    {
        public long FilialId { get; set; }
        public List<HorarioFuncionamentoDto> Horarios { get; set; } = [];
    }
}
