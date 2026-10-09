using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Restaurant.Api.Hubs
{
    [Authorize(Roles = "Admin,Manager,Waiter")]
    public class RestaurantHub : Hub
    {
    }
}
