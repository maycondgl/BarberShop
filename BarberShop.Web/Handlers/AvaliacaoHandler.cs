using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Avaliacao;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Avaliacao;

namespace BarberShop.Web.Handlers
{
    public class AvaliacaoHandler : IAvaliacaoHandler
    {
        public Task<Response<AvaliacaoResponse?>> CreateAsync(CreateAvaliacaoRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Response<AvaliacaoResponse?>> DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResponse<List<Avaliacao>>> GetAllAsync(GetAllAvaliacaoRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Response<Avaliacao?>> GetByIdAsync(GetAvaliacaoByIdRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Response<AvaliacaoResponse?>> UpdateAsync(UpdateAvaliacaoRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
