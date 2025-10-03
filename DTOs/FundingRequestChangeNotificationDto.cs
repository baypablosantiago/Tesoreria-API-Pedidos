namespace API_Pedidos.DTOs
{
    public class FundingRequestChangeNotificationDto
    {
        public long RequestId { get; set; }
        public int RequestNumber { get; set; }
        public int DA { get; set; }
        public string ChangeType { get; set; } = string.Empty; // "CREATE", "UPDATE", "STATUS_CHANGE", "WORK_STATUS_CHANGE", "COMMENT_ADDED", "PAYMENT_UPDATED"
        public string? FieldChanged { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime ChangeDate { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public FundingRequestAdminResponseDto FullRequest { get; set; } = null!;
    }
}
