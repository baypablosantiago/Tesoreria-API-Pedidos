using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace API_Pedidos.Hubs
{
    [Authorize(Roles = "admin")]
    public class FundingRequestHub : Hub
    {
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admins");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
