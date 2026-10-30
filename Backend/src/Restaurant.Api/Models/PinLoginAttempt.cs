namespace Restaurant.Api.Models
{
    public class PinLoginAttempt
    {
        public int Id { get; set; }
        public string ClientKey { get; set; } = string.Empty; 
        public int FailedCount { get; set; } 
        public DateTime? BlockedUntil { get; set; } 
        public DateTime LastAttemptAt { get; set; }
    }
}
