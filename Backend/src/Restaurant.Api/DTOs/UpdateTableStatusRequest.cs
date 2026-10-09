using Restaurant.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class UpdateTableStatusRequest
    {
        [EnumDataType(typeof(TableStatus))]
        public TableStatus Status { get; set; }
    }
}
