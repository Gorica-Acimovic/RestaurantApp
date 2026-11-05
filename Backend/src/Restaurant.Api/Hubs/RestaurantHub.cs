using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Restaurant.Api.Hubs
{
    [Authorize(Roles = "Admin,Manager,Waiter, Guest, Kitchen, Courier")]
    public class RestaurantHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var role = Context.User?.FindFirstValue(ClaimTypes.Role);

            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");


            if (role is "Admin" or "Manager")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "managers");
            }
            else if (role == "Waiter")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "waiters");
            }
            else if (role == "Kitchen")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "kitchen");
            }
            else if (role == "Courier")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "couriers");
            }

            await base.OnConnectedAsync();
        }

        [Authorize(Roles = "Admin,Manager,Waiter")]
        public async Task JoinTable(int tableId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"table-{tableId}");
        }

        [Authorize(Roles = "Admin,Manager,Waiter")]
        public async Task LeaveTable(int tableId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"table-{tableId}");
        }
    }
}
