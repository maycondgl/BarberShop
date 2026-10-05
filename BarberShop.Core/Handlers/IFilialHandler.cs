using BarberShop.Core.Models;
using BarberShop.Core.Requests.Filiais;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Filial;

namespace BarberShop.Core.Handlers
{
    public interface IFilialHandler
    {
        Task<Response<FilialResponse?>> CreateAsync(CreateFilialRequest request);
        Task<Response<FilialResponse?>> UpdateAsync(UpdateFilialRequest request);
        Task<Response<FilialResponse?>> DeleteAsync(long id);
        Task<Response<Filial?>> GetByIdAsync(GetFilialByIdRequest request);
        Task<PagedResponse<List<Filial>>> GetAllAsync(GetAllFilialRequest request);
    }
}
