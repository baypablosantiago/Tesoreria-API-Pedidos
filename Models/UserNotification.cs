namespace API_Pedidos.Models
{
    public class UserNotification
    {
        public long Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public long RequestId { get; set; }
        public string ChangeType { get; set; } = string.Empty; // COMMENT_ADDED, WORK_STATUS_CHANGE, STATUS_FINALIZED
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public FundingRequest? Request { get; set; }
    }
}
