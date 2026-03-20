using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace mperformancepower.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        await base.OnConnectedAsync();
    }
}
