using BarberShop.Core.Models;
using BarberShop.Core.Requests.DiasFechados;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.DiasFechados;

namespace BarberShop.Core.Handlers
{
    public interface IDiaFechadoHandler
    {
        Task<Response<DiaFechadoResponse?>> CreateAsync(CreateDiaFechadoRequest request);
        Task<Response<DiaFechadoResponse?>> DeleteAsync(long id);
        Task<PagedResponse<List<DiaFechado>>> GetAllAsync(GetAllDiasFechadosRequest request);
    }
}
