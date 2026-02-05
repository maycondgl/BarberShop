using BarberShop.Core.Models;
using BarberShop.Core.Requests.Avaliacao;
using BarberShop.Core.Responses;

namespace BarberShop.Core.Handlers
{
    public interface IAvaliacaoHandler
    {
        Task<Response<Avaliacao>> CreateAsync(CreateAvaliacaoRequest request);
        Task<Response<Avaliacao>> UpdateAsync(UpdateAvaliacaoRequest request);
        Task<Response<Avaliacao>> DeleteAsync(DeleteAvaliacaoRequest request);
        Task<Response<Avaliacao>> GetByIdAsync(GetAvaliacaoByIdRequest request);
        Task<Response<List<Avaliacao>>> GetAllAsync(GetAllAvaliacaoRequest request);
    }
}
