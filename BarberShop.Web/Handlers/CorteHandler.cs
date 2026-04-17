using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Cortes;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Corte;

namespace BarberShop.Web.Handlers
{
    public class CorteHandler : ICorteHandler
    {
        public Task<Response<CorteResponse?>> CreateAsync(CreateCorteRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Response<CorteResponse?>> DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResponse<List<Corte>>> GetAllAsync(GetAllCorteRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Response<Corte?>> GetByIdAsync(GetCorteByIdRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Response<CorteResponse?>> UpdateAsync(UpdateCorteRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
