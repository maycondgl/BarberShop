using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Dashboard;

namespace BarberShop.Core.Handlers;

public interface IDashboardHandler
{
    Task<Response<DashboardResponse?>> GetDashboardAsync(long? filialId = null);
}
