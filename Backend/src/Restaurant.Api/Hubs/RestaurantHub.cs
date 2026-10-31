using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Restaurant.Api.Hubs
{
    [Authorize(Roles = "Admin,Manager,Waiter")]
    public class RestaurantHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var role = Context.User?.FindFirstValue(ClaimTypes.Role);

            if (role is "Admin" or "Manager")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "managers");
            }
            else if (role == "Waiter")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "waiters");
            }

            await base.OnConnectedAsync();
        }

        public async Task JoinTable(int tableId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"table-{tableId}");
        }

        public async Task LeaveTable(int tableId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"table-{tableId}");
        }
    }
}
