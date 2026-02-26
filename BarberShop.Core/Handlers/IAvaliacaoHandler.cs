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
        Task<PagedResponse<List<Avaliacao>>> GetAllAsync(GetAllAvaliacaoRequest request);
    }
}
