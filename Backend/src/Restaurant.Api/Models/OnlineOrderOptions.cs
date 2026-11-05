namespace Restaurant.Api.Models
{
    public class OnlineOrderOptions
    {
        public decimal DeliveryFee { get; set; } = 2.00m;
        public int MaxActiveOrdersPerGuest { get; set; } = 3;
        public int MaxLinesPerOrder { get; set; } = 30;
    }
}
