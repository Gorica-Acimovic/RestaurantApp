using Restaurant.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class CloseOrderRequest
    {
        [EnumDataType(typeof(PaymentMethod), ErrorMessage = "Payment method must be Cash or Card.")]
        public PaymentMethod PaymentMethod { get; set; }
    }
}
