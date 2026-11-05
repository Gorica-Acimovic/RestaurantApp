using Restaurant.Api.Models;

namespace Restaurant.Api.DTOs
{
    public class ChangeOnlineOrderStatusRequest
    {
        public DeliveryStatus Status { get; set; }
    }
}
