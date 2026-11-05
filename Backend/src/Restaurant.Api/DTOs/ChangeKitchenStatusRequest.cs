using Restaurant.Api.Models;

namespace Restaurant.Api.DTOs
{
    public class ChangeKitchenStatusRequest
    {
        public OrderItemStatus Status { get; set; }
    }
}
