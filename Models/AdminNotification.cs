namespace API_Pedidos.Models
{
    public class AdminNotification
    {
        public long Id { get; set; }
        public string AdminUserId { get; set; } = string.Empty;
        public long RequestId { get; set; }
        public int RequestNumber { get; set; }
        public int DA { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public string? FieldChanged { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public DateTime ChangeDate { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public FundingRequest? Request { get; set; }
    }
}
