using BarberShop.Core.Models;
using BarberShop.Core.Requests.Clientes;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Cliente;

namespace BarberShop.Core.Handlers
{
    public interface IClienteHandler
    {
        Task<Response<ClienteResponse?>> CreateAsync(CreateClienteRequest request);
        Task<Response<ClienteResponse?>> UpdateAsync(UpdateClienteRequest request);
        Task<Response<ClienteResponse?>> DeleteAsync(long id);
        Task<Response<Cliente?>> GetByIdAsync(GetClienteByIdRequest request);
        Task<Response<List<ClienteResponse?>>> GetAllAsync(GetAllClienteRequest request);
    }
}
