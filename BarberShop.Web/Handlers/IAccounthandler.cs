using BarberShop.Core.Requests.Account;
using BarberShop.Core.Responses;

namespace BarberShop.Web.Handlers
{
    public interface IAccounthandler
    {
        Task<Response<string>> UpdateProfileAsync(UpdateProfileRequest request);
        Task<Response<string>> DeleteProfileAsync();
    }
}