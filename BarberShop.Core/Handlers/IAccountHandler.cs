using BarberShop.Core.Requests.Account;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Account;

namespace BarberShop.Core.Handlers
{
    public interface IAccountHandler
    {
        Task<Response<string>> LoginAsync(LoginRequest request);
        Task<Response<string>> RegisterAsync(RegisterRequest request);
        Task LogoutAsync();
        Task<Response<List<AdminUserResponse>>> GetUsersAsync();
        Task<Response<string>> AddAdminAsync(long userId);
        Task<Response<string>> RemoveAdminAsync(long userId);
    }
}
