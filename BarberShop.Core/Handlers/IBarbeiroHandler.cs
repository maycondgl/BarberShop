using BarberShop.Core.Models;
using BarberShop.Core.Requests.Barbeiros;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Barbeiro;

namespace BarberShop.Core.Handlers
{
    public interface IBarbeiroHandler
    {
        Task<Response<BarbeiroResponse?>> CreateAsync(CreateBarbeiroRequest request);
        Task<Response<BarbeiroResponse?>> UpdateAsync(UpdateBarbeiroRequest request);
        Task<Response<BarbeiroResponse?>> DeleteAsync(long id);
        Task<Response<Barbeiro?>> GetByIdAsync(GetBarbeiroByIdRequest request);
        Task<Response<Barbeiro?>> GetByUserIdAsync(long userId);
        Task<PagedResponse<List<Barbeiro>>> GetAllAsync(GetAllBarbeiroRequest request);
        Task<Response<BarbeiroDashboardResponse?>> GetDashboardAsync(long? barbeiroId = null);
        Task<Response<BarbeiroUsuarioInfoResponse?>> BuscarUsuarioPorEmailAsync(string email);
    }
}
