using BarberShop.Core.Models;
using BarberShop.Core.Requests.Avaliacao;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Avaliacao;

namespace BarberShop.Core.Handlers
{
    public interface IAvaliacaoHandler
    {
        Task<Response<AvaliacaoResponse?>> CreateAsync(CreateAvaliacaoRequest request);
        Task<Response<AvaliacaoResponse?>> UpdateAsync(UpdateAvaliacaoRequest request);
        Task<Response<AvaliacaoResponse?>> DeleteAsync(long id);
        Task<Response<Avaliacao?>> GetByIdAsync(GetAvaliacaoByIdRequest request);
        Task<PagedResponse<List<AvaliacaoResponse>>> GetAllAsync(GetAllAvaliacaoRequest request);
        Task<PagedResponse<List<AvaliacaoResponse>>> GetAllPublicAsync(int pageNumber, int pageSize);
        Task<PagedResponse<List<AvaliacaoResponse>>> GetByBarbeiroAsync(long barbeiroId, int pageNumber = 1, int pageSize = 50);
        Task<PagedResponse<List<AvaliacaoResponse>>> GetAllAdminAsync(int pageNumber = 1, int pageSize = 50);
    }
}
