using BarberShop.Core.Requests.HorariosFuncionamento;
using BarberShop.Core.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Core.Handlers
{
    public interface IHorarioFuncionamentoHandler
    {
        Task<Response<List<HorarioFuncionamentoDto>>> GetByFilialAsync(long filialId);
        Task<Response<bool>> SaveHorariosAsync(SalvarHorariosFilialRequest request);
    }
}
