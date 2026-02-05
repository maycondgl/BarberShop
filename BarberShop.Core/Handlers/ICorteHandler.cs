using BarberShop.Core.Models;
using BarberShop.Core.Requests.Cortes;
using BarberShop.Core.Responses;

namespace BarberShop.Core.Handlers
{
    public interface ICorteHandler
    {
        Task<Response<Corte>> CreateAsync(CreateCorteRequest request);
        Task<Response<Corte>> UpdateAsync(UpdateCorteRequest request);
        Task<Response<Corte>> DeleteAsync(DeleteCorteRequest request);
        Task<Response<Corte>> GetByIdAsync(GetCorteByIdRequest request);
        Task<Response<List<Corte>>> GetAllAsync(GetAllCorteRequest request);
    }
}
