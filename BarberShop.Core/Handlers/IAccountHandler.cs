using BarberShop.Core.Requests.Account;
using BarberShop.Core.Responses;

namespace BarberShop.Core.Handlers
{
    public interface IAccountHandler
    {
        Task<Response<string>> LoginAsync(LoginRequest request);
        Task<Response<string>> RegisterAsync(RegisterRequest request);
        Task LogoutAsync();
    }
}
