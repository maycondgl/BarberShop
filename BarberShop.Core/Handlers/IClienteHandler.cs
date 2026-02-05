using BarberShop.Core.Models;
using BarberShop.Core.Requests.Clientes;
using BarberShop.Core.Responses;

namespace BarberShop.Core.Handlers
{
    public interface IClienteHandler
    {
        Task<Response<Cliente>> CreateAsync(CreateClienteRequest request);
        Task<Response<Cliente>> UpdateAsync(UpdateClienteRequest request);
        Task<Response<Cliente>> DeleteAsync(DeleteClienteRequest request);
        Task<Response<Cliente>> GetByIdAsync(GetClienteByIdRequest request);
        Task<Response<List<Cliente>>> GetAllAsync(GetAllClienteRequest request);
    }
}
