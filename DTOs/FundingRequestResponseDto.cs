namespace API_Pedidos.DTOs
{
    public class FundingRequestResponseDto
    {
        public long Id { get; set; }
        public DateTime ReceivedAt { get; set; }
        public int DA { get; set; }
        public int RequestNumber { get; set; }
        public int FiscalYear { get; set; }
        public string PaymentOrderNumber { get; set; } = string.Empty;
        public string Concept { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string FundingSource { get; set; } = string.Empty;
        public string CheckingAccount { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public string? CommentsFromTeso { get; set; }
        public double PartialPayment { get; set; }
        public bool IsActive { get; set; }
        public bool OnWork { get; set; }
    }
}