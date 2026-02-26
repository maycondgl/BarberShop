using BarberShop.Core.Models;
using BarberShop.Core.Requests.Cortes;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Corte;

namespace BarberShop.Core.Handlers
{
    public interface ICorteHandler
    {
        Task<Response<CorteResponse?>> CreateAsync(CreateCorteRequest request);
        Task<Response<CorteResponse?>> UpdateAsync(UpdateCorteRequest request);
        Task<Response<CorteResponse?>> DeleteAsync(long id);
        Task<Response<Corte?>> GetByIdAsync(GetCorteByIdRequest request);
        Task<Response<List<CorteResponse?>>> GetAllAsync(GetAllCorteRequest request);
    }
}
