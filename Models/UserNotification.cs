namespace API_Pedidos.Models
{
    public class UserNotification
    {
        public long Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public long RequestId { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public FundingRequest? Request { get; set; }
    }
}
