using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BarberShop.Api.Hubs;

[Authorize(Policy = "Admin")]
public class AgendamentoHub : Hub
{
    public const string AdminGroupName = "Admins";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroupName);
        await base.OnConnectedAsync();
    }
}
